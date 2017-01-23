namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class Registration : Token<IRegistration>, IRegistration
    {
        private readonly List<HashSet<IContractKey>> _contractKeys = new List<HashSet<IContractKey>>();
        private readonly HashSet<ITagKey> _tagKeys = new HashSet<ITagKey>();
        private readonly HashSet<IStateKey> _stateKeys = new HashSet<IStateKey>();
        private readonly HashSet<ICompositeKey> _compositeKeys = new HashSet<ICompositeKey>();
        private readonly Lazy<IInstanceFactoryProvider> _instanceFactoryProvider;

        public Registration(IFluent fluent, IResolver resolver)
            : base(fluent, resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _instanceFactoryProvider = new Lazy<IInstanceFactoryProvider>(GetInstanceFactoryProvider);
        }

        private List<IExtension> Extensions { get; } = new List<IExtension>();

        public override IRegistration Contract(params Type[] contractTypes)
        {
            AddContractKey(contractTypes.Select(type => KeyFactory.CreateContractKey(type, false)));
            return this;
        }

        public IRegistration Lifetime(ILifetime lifetime)
        {
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            Extensions.Add(lifetime);
            return this;
        }

        public IRegistration Lifetime(Wellknown.Lifetimes lifetime)
        {
            Extensions.Add(Fluent.Resolve().Tag(lifetime).Instance<ILifetime>());
            return this;
        }

        public IRegistration KeyComparer(IKeyComparer keyComparer)
        {
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));
            Extensions.Add(keyComparer);
            return this;
        }

        public IRegistration KeyComparer(Wellknown.KeyComparers keyComparer)
        {
            Extensions.Add(Fluent.Resolve().Tag(keyComparer).Instance<IKeyComparer>());
            return this;
        }

        public IRegistration Scope(IScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (Extensions.OfType<IScope>().Any()) throw new InvalidOperationException();
            Extensions.Add(scope);
            return this;
        }

        public IRegistration Scope(Wellknown.Scopes scope)
        {
            Extensions.Add(Fluent.Resolve().Tag(scope).Instance<IScope>());
            return this;
        }

        public IDisposable AsFactoryMethod(Func<IResolverContext, object> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            CeateCompositeKeys();
            return AsFactoryMethodInternal(factoryMethod);
        }

        public IDisposable AsFactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            Contract<TImplementation>();
            return AsFactoryMethodInternal(factoryMethod);
        }

        public IDisposable AsAutowiring(Type implementationType, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            metadataProvider = metadataProvider ?? AutowiringMetadataProvider.Shared;
            return AsFactoryMethod(ctx =>
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
            });
        }

        public IDisposable AsAutowiring<TImplementation>()
        {
            return AsAutowiring(typeof(TImplementation));
        }

        protected override bool AddContractKey(IEnumerable<IContractKey> keys)
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

        private void CeateCompositeKeys()
        {
            foreach (var contractKeys in _contractKeys)
            {
                var generics = contractKeys.OrderBy(i => i.ContractType.FullName).ToArray();
                _compositeKeys.Add(KeyFactory.CreateCompositeKey(
                    generics,
                    _tagKeys.Count > 0 ? _tagKeys.OrderBy(i => i.Tag).ToArray() : EmptyTagKeys,
                    _stateKeys.Count > 0 ? _stateKeys.OrderBy(i => i.Index).ToArray() : EmptyStateKeys));
            }

            _contractKeys.Clear();
            _stateKeys.Clear();
            _tagKeys.Clear();
        }

        private IDisposable AsFactoryMethodInternal<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            CeateCompositeKeys();
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