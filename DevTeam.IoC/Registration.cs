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
        private readonly HashSet<IKey> _registrationKeys = new HashSet<IKey>();
        private readonly Lazy<IInstanceFactoryProvider> _instanceFactoryProvider;
        private readonly RegistrationResult<T> _result;
        private readonly ICache<Type, IResolverFactory> _resolverFactoryCache;

        public Registration([NotNull] IFluent fluent, [NotNull] T container)
            : base(container)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (container == null) throw new ArgumentNullException(nameof(container));
            _instanceFactoryProvider = new Lazy<IInstanceFactoryProvider>(GetInstanceFactoryProvider);
            _result = new RegistrationResult<T>(this);
            var cacheProvider = container as IProvider<ICache<Type, IResolverFactory>>;
            cacheProvider?.TryGet(out _resolverFactoryCache);
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
            AsFactoryMethodInternal(factoryMethod);
            return _result;
        }

        public IRegistrationResult<T> FactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            AsFactoryMethodInternal(factoryMethod, typeof(TImplementation));
            return _result;
        }

        public IRegistrationResult<T> Autowiring(Type implementationType, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            metadataProvider = metadataProvider ?? Fluent.Resolve(Resolver).Instance<IMetadataProvider>();
            AsFactoryMethodInternal(ctx =>
                {
                    var resolvedType = metadataProvider.ResolveImplementationType(ctx, implementationType);
                    IResolverFactory factory;
                    if (_resolverFactoryCache != null)
                    {
                        if (!_resolverFactoryCache.TryGet(resolvedType, out factory))
                        {
                            factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider);
                            _resolverFactoryCache.Set(resolvedType, factory);
                        }
                    }
                    else
                    {
                        factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider);
                    }

                    return factory.Create(ctx);
                },
                implementationType);
            return _result;
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
            return _registrationKeys.Add(compositeKey);
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

        private void AppendRegistryKeys()
        {
            foreach (var contractKeys in _contractKeys)
            {
                var tagKeys = _tagKeys.Any() ? _tagKeys : null;
                var stateKeys = _stateKeys.Any() ? _stateKeys : null;
                if (contractKeys.Count == 1 && tagKeys == null && stateKeys == null)
                {
                    _registrationKeys.Add(contractKeys.Single());
                }
                else
                {
                    _registrationKeys.Add(Resolver.KeyFactory.CreateCompositeKey(contractKeys, tagKeys, stateKeys));
                }
            }

            _contractKeys.Clear();
            _stateKeys.Clear();
            _tagKeys.Clear();
        }

        private void AsFactoryMethodInternal<TImplementation>(
            Func<IResolverContext, TImplementation> factoryMethod,
            Type implementationType = null)
        {
            AppendRegistryKeys();
            if (ExtractMetadata(implementationType ?? typeof(TImplementation)))
            {
                AppendRegistryKeys();
            }

            IDisposable registration;
            var context = Resolver.CreateRegistryContext(_registrationKeys, new MethodFactory<TImplementation>(factoryMethod), Extensions);
            if (!Resolver.TryRegister(context, out registration))
            {
                throw new InvalidOperationException($"Can't register {string.Join(Environment.NewLine, context.Keys)}.{Environment.NewLine}{Environment.NewLine}{Resolver}");
            }

            _registrationKeys.Clear();
            _result.AddResource(registration);
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