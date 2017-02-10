namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class Registration<T> : Token<T, IRegistration<T>>, IRegistration<T>
         where T : IResolver
    {
        private readonly List<HashSet<IContractKey>> _contractKeys = new List<HashSet<IContractKey>>();
        private readonly HashSet<ITagKey> _tagKeys = new HashSet<ITagKey>();
        private readonly HashSet<IStateKey> _stateKeys = new HashSet<IStateKey>();
        private readonly HashSet<ICompositeKey> _compositeKeys = new HashSet<ICompositeKey>();
        private readonly Lazy<IInstanceFactoryProvider> _instanceFactoryProvider;

        public Registration([NotNull] IFluent fluent, [NotNull] T resolver)
            : base(fluent, resolver)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _instanceFactoryProvider = new Lazy<IInstanceFactoryProvider>(GetInstanceFactoryProvider);
        }

        private List<IExtension> Extensions { get; } = new List<IExtension>();

        public IRegistration<T> Attributes(Type implementationType)
        {
            ExtractMetadata(implementationType);
            return this;
        }

        public IRegistration<T> Attributes<TImplementation>()
        {
            return Attributes(typeof(TImplementation));
        }

        public override IRegistration<T> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => KeyFactory.CreateContractKey(type, false)));
            return this;
        }

        public IRegistration<T> Lifetime(ILifetime lifetime)
        {
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            Extensions.Add(lifetime);
            return this;
        }

        public IRegistration<T> Lifetime(Wellknown.Lifetime lifetime)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(lifetime).Instance<ILifetime>());
            return this;
        }

        public IRegistration<T> KeyComparer(IKeyComparer keyComparer)
        {
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));
            Extensions.Add(keyComparer);
            return this;
        }

        public IRegistration<T> KeyComparer(Wellknown.KeyComparer keyComparer)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(keyComparer).Instance<IKeyComparer>());
            return this;
        }

        public IRegistration<T> Scope(IScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (Extensions.OfType<IScope>().Any()) throw new InvalidOperationException();
            Extensions.Add(scope);
            return this;
        }

        public IRegistration<T> Scope(Wellknown.Scope scope)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(scope).Instance<IScope>());
            return this;
        }

        public IDisposable FactoryMethod(Func<IResolverContext, object> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            return AsFactoryMethodInternal(factoryMethod);
        }

        public IConfiguring<T> AsFactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            return Own(FactoryMethod(factoryMethod)).Configure();
        }

        public IDisposable FactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            return AsFactoryMethodInternal(factoryMethod, typeof(TImplementation));
        }

        public IConfiguring<T> AsFactoryMethod(Func<IResolverContext, object> factoryMethod)
        {
            return Own(FactoryMethod(factoryMethod)).Configure();
        }

        public IDisposable Autowiring(Type implementationType, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            metadataProvider = metadataProvider ?? Fluent.Resolve(Resolver).Instance<IMetadataProvider>();
            return AsFactoryMethodInternal(ctx =>
                {
                    var resolvedType = metadataProvider.ResolveImplementationType(ctx, implementationType);
                    ICache<Type, IResolverFactory> factoryCache;
                    IResolverFactory factory;
                    if (ctx.Container.TryResolve(out factoryCache))
                    {
                        if (!factoryCache.TryGet(resolvedType, out factory))
                        {
                            factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider);
                            factoryCache.Set(resolvedType, factory);
                        }
                    }
                    else
                    {
                        factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider);
                    }

                    return factory.Create(ctx);
                },
                implementationType);
        }

        public IConfiguring<T> AsAutowiring(Type implementationType, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            return Own(Autowiring(implementationType, metadataProvider)).Configure();
        }

        public IDisposable Autowiring<TImplementation>()
        {
            return Autowiring(typeof(TImplementation));
        }

        public IConfiguring<T> AsAutowiring<TImplementation>()
        {
            return Own(Autowiring<TImplementation>()).Configure();
        }

        protected override bool AddContractKey([NotNull] IEnumerable<IContractKey> keys)
        {
            _contractKeys.Add(new HashSet<IContractKey>(keys));
            return true;
        }

        protected override bool AddStateKey(IStateKey key)
        {
            return _stateKeys.Add(key);
        }

        protected override bool AddTagKey(ITagKey key)
        {
            return _tagKeys.Add(key);
        }

        protected override bool AddCompositeKey(ICompositeKey compositeKey)
        {
            return _compositeKeys.Add(compositeKey);
        }

        private T Own(IDisposable resource)
        {
            Resolver.Resolve().Instance<IInternalResourceStore>().AddResource(resource);
            return Resolver;
        }

        private bool ExtractMetadata(Type metadataType)
        {
            if (metadataType == typeof(object))
            {
                return false;
            }

            var hasMetadata = false;
            var typeInfo = metadataType.GetTypeInfo();
            foreach (var contract in typeInfo.GetCustomAttributes<ContractAttribute>())
            {
                Contract(contract.ContractTypes);
                hasMetadata = true;
            }

            foreach (var state in typeInfo.GetCustomAttributes<StateAttribute>())
            {
                State(state.Index, state.StateType);
                hasMetadata = true;
            }

            foreach (var tag in typeInfo.GetCustomAttributes<TagAttribute>())
            {
                Tag(tag.Tags);
                hasMetadata = true;
            }

            return hasMetadata;
        }

        private void AppendCompositeKeys()
        {
            foreach (var contractKeys in _contractKeys)
            {
                var generics = contractKeys.OrderBy(i => i.ContractType.FullName);
                _compositeKeys.Add(KeyFactory.CreateCompositeKey(generics, _tagKeys, _stateKeys));
            }

            _contractKeys.Clear();
            _stateKeys.Clear();
            _tagKeys.Clear();
        }

        private IDisposable AsFactoryMethodInternal<TImplementation>(
            Func<IResolverContext,
            TImplementation> factoryMethod,
            Type implementationType = null)
        {
            AppendCompositeKeys();
            if (ExtractMetadata(implementationType ?? typeof(TImplementation)))
            {
                AppendCompositeKeys();
            }

            IDisposable registration;
            var context = Registry.CreateContext(_compositeKeys, new MethodFactory<TImplementation>(factoryMethod), Extensions);
            if (!Registry.TryRegister(context, out registration))
            {
                throw new InvalidOperationException($"Can't register {string.Join(Environment.NewLine, context.Keys)}.{Environment.NewLine}{Environment.NewLine}{Registry}");
            }

            return registration;
        }

        private IInstanceFactoryProvider GetInstanceFactoryProvider()
        {
            IInstanceFactoryProvider instanceFactoryProvider;
            if (!Resolver.TryResolve(out instanceFactoryProvider))
            {
                throw new InvalidOperationException($"{typeof(IInstanceFactoryProvider)} was not registered.");
            }

            return instanceFactoryProvider;
        }
    }
}