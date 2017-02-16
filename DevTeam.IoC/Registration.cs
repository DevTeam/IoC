namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class Registration<T> : Token<T, IRegistration<T>>, IRegistration<T>
          where T : IContainer
    {
        private readonly List<HashSet<IContractKey>> _contractKeys = new List<HashSet<IContractKey>>();
        private readonly HashSet<ITagKey> _tagKeys = new HashSet<ITagKey>();
        private readonly HashSet<IStateKey> _stateKeys = new HashSet<IStateKey>();
        private readonly HashSet<ICompositeKey> _compositeKeys = new HashSet<ICompositeKey>();
        private readonly Lazy<IInstanceFactoryProvider> _instanceFactoryProvider;

        public Registration([NotNull] IFluent fluent, [NotNull] T container)
            : base(fluent, container)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (container == null) throw new ArgumentNullException(nameof(container));
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
            AddContractKey(contractTypes.Select(type => Resolver.KeyFactory.CreateContractKey(type, false)));
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

        public IRegistrationResult<T> FactoryMethod(Func<IResolverContext, object> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            return AsFactoryMethodInternal(factoryMethod);
        }

        public IRegistrationResult<T> FactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            return AsFactoryMethodInternal(factoryMethod, typeof(TImplementation));
        }

        public IRegistrationResult<T> Autowiring(Type implementationType, IMetadataProvider metadataProvider = null)
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

        public IRegistrationResult<T> Autowiring<TImplementation>()
        {
            return Autowiring(typeof(TImplementation));
        }

        internal T ToSelf(params IDisposable[] resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            Resolver.Resolve().Instance<IInternalResourceStore>().AddResource(new CompositeDisposable(resource));
            return Resolver;
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
                _compositeKeys.Add(Resolver.KeyFactory.CreateCompositeKey(contractKeys, _tagKeys.Any() ? _tagKeys : null, _stateKeys.Any() ? _stateKeys : null));
            }

            _contractKeys.Clear();
            _stateKeys.Clear();
            _tagKeys.Clear();
        }

        private IRegistrationResult<T> AsFactoryMethodInternal<TImplementation>(
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
            var context = Resolver.CreateContext(_compositeKeys, new MethodFactory<TImplementation>(factoryMethod), Extensions);
            return new RegistrationResult<T>(
                this,
                () => {
                    if (!Resolver.TryRegister(context, out registration))
                    {
                        throw new InvalidOperationException(
                            $"Can't register {string.Join(Environment.NewLine, context.Keys)}.{Environment.NewLine}{Environment.NewLine}{Resolver}");
                    }

                    return registration;
                });
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