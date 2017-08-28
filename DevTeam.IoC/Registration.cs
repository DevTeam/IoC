namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal sealed class Registration<TContainer> : Token<TContainer, IRegistration<TContainer>>, IRegistration<TContainer>
          where TContainer : IContainer
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IExtension[] EmptyExtensions = new IExtension[0];

        [CanBeNull] private Registration<TContainer> _defaultRegistration;
        [NotNull] private readonly Lazy<IMethodFactory> _instanceFactoryProvider;
        [NotNull] private readonly RegistrationResult<TContainer> _result;
        [NotNull] private readonly IReflection _reflection;
        [CanBeNull] private readonly ICache<Type, IInstanceFactory> _resolverFactoryCache;

        [NotNull] private readonly HashSet<IKey> _registrationKeys;
        [NotNull] private readonly List<HashSet<IContractKey>> _contractKeys;
        [CanBeNull] private HashSet<ITagKey> _tagKeys;
        [CanBeNull] private HashSet<IStateKey> _stateKeys;
        [CanBeNull] private List<IExtension> _extensions;

        internal Registration(
            [NotNull] IFluent fluent,
            [NotNull] TContainer container,
            [CanBeNull] Registration<TContainer> defaultRegistration = null)
            : base(container)
        {
            _instanceFactoryProvider = new Lazy<IMethodFactory>(GetInstanceFactoryProvider);
            _result = new RegistrationResult<TContainer>(this);
            var cacheProvider = container as IProvider<ICache<Type, IInstanceFactory>>;
            cacheProvider?.TryGet(out _resolverFactoryCache);
            _reflection = container.Resolve().Instance<IReflection>();

            if (defaultRegistration != null)
            {
                _defaultRegistration = defaultRegistration;
                _registrationKeys = new HashSet<IKey>(defaultRegistration._registrationKeys);
                _contractKeys = new List<HashSet<IContractKey>>(defaultRegistration._contractKeys);
                if (defaultRegistration._stateKeys != null)
                {
                    _stateKeys = new HashSet<IStateKey>(defaultRegistration._stateKeys);
                }

                if (defaultRegistration._tagKeys != null)
                {
                    _tagKeys = new HashSet<ITagKey>(defaultRegistration._tagKeys);
                }

                if (defaultRegistration._extensions != null)
                {
                    _extensions = new List<IExtension>(defaultRegistration._extensions);
                }
            }
            else
            {
                _registrationKeys = new HashSet<IKey>();
                _contractKeys = new List<HashSet<IContractKey>>();
            }
        }

        internal IEnumerable<IKey> Keys => GetRegistryKeys();

        internal List<IExtension> Extensions => _extensions ?? (_extensions = new List<IExtension>());

        public IRegistration<TContainer> Attributes(Type implementationType)
        {
            ExtractMetadata(implementationType);
            return this;
        }

        public IRegistration<TContainer> Attributes<TImplementation>()
        {
            return Attributes(typeof(TImplementation));
        }

        public override IRegistration<TContainer> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => Resolver.KeyFactory.CreateContractKey(type, false)));
            return this;
        }

        public override IRegistration<TContainer> State(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            AddStateKey(Resolver.KeyFactory.CreateStateKey(index, stateType, false));
            return this;
        }

        public IRegistration<TContainer> Lifetime(ILifetime lifetime)
        {
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));
            Extensions.Add(lifetime);
            return this;
        }

        public IRegistration<TContainer> Lifetime(Wellknown.Lifetime lifetime)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(lifetime).Instance<ILifetime>());
            return this;
        }

        public IRegistration<TContainer> KeyComparer(IKeyComparer keyComparer)
        {
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));
            Extensions.Add(keyComparer);
            return this;
        }

        public IRegistration<TContainer> KeyComparer(Wellknown.KeyComparer keyComparer)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(keyComparer).Instance<IKeyComparer>());
            return this;
        }

        public IRegistration<TContainer> Scope(IScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (Extensions.OfType<IScope>().Any())
            {
                throw new ContainerException($"Only one scope is allowed. {string.Join(", ", Extensions.Select(i => i.ToString()).ToArray())} are already defined.");
            }

            Extensions.Add(scope);
            return this;
        }

        public IRegistration<TContainer> Scope(Wellknown.Scope scope)
        {
            Extensions.Add(Fluent.Resolve(Resolver).Tag(scope).Instance<IScope>());
            return this;
        }

        public IRegistrationResult<TContainer> FactoryMethod(Func<CreationContext, object> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            FactoryMethodInternal(factoryMethod);
            return _result;
        }

        public IRegistrationResult<TContainer> FactoryMethod<TImplementation>(Func<CreationContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            FactoryMethodInternal(factoryMethod, typeof(TImplementation));
            return _result;
        }

        public IRegistrationResult<TContainer> Autowiring(Type implementationType, bool lazy = false, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            metadataProvider = metadataProvider ?? Fluent.Resolve(Resolver).Instance<IMetadataProvider>();
            IInstanceFactory instanceFactory;
            if (!lazy && metadataProvider.TryResolveType(implementationType, out var resolvedType))
            {
                instanceFactory = CreateFactory(resolvedType, metadataProvider);
                FactoryMethodInternal(ctx => instanceFactory.Create(ctx), implementationType);
            }
            else
            {
                FactoryMethodInternal(ctx =>
                    {
                        if (!metadataProvider.TryResolveType(implementationType, out var currentResolvedType, ctx))
                        {
                            throw new ContainerException($"Can not define a type to resolve from type \"{implementationType}\"");
                        }

                        return CreateFactory(currentResolvedType, metadataProvider).Create(ctx);
                    },
                    implementationType);
            }

            return _result;
        }

        public IRegistrationResult<TContainer> Autowiring<TImplementation>(bool lazy = false)
        {
            return Autowiring(typeof(TImplementation), lazy);
        }

        public IRegistrationResult<TContainer> Autowiring<TContract, TImplementation>(params object[] tags)
             where TImplementation : TContract
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            Contract<TContract>();
            Tag(tags);
            return Autowiring(typeof(TImplementation));
        }

        public IRegistrationResult<TContainer> Autowiring(Type contractType, Type implementationType, params object[] tags)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            Contract(contractType);
            Tag(tags);
            return Autowiring(implementationType);
        }

        public IRegistration<TContainer> With()
        {
            _defaultRegistration = new Registration<TContainer>(Fluent, Resolver, this);
            return new Registration<TContainer>(Fluent, Resolver, _defaultRegistration);
        }

        internal TContainer ToSelf(params IDisposable[] resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            Resolver.Resolve().Instance<IInternalResourceStore>().AddResource(new CompositeDisposable(resource));
            return Resolver;
        }

        internal Registration<TContainer> New()
        {
            return new Registration<TContainer>(Fluent, Resolver, _defaultRegistration);
        }

        protected override bool AddContractKey([NotNull] IEnumerable<IContractKey> keys)
        {
            _contractKeys.Add(new HashSet<IContractKey>(keys));
            return true;
        }

        protected override bool AddStateKey(IStateKey key)
        {
            if (_stateKeys == null)
            {
                _stateKeys = new HashSet<IStateKey>();
            }

            return _stateKeys.Add(key);
        }

        protected override bool AddTagKey(ITagKey key)
        {
            if (_tagKeys == null)
            {
                _tagKeys = new HashSet<ITagKey>();
            }

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
            var typeInfo = _reflection.GetType(metadataType);
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

        private IEnumerable<IKey> GetRegistryKeys()
        {
            foreach (var contractKeys in _contractKeys)
            {
                if (contractKeys.Count == 1 && _tagKeys == null && _stateKeys == null)
                {
                    yield return contractKeys.Single();
                }
                else
                {
                    yield return Resolver.KeyFactory.CreateCompositeKey(contractKeys, _tagKeys, _stateKeys);
                }
            }
        }

        private void AppendRegistryKeys()
        {
            foreach (var registryKey in GetRegistryKeys())
            {
                _registrationKeys.Add(registryKey);
            }

            _contractKeys.Clear();
            _stateKeys?.Clear();
            _tagKeys?.Clear();
        }

        private void FactoryMethodInternal<TImplementation>(
            Func<CreationContext, TImplementation> factoryMethod,
            Type implementationType = null)
        {
            AppendRegistryKeys();
            if (ExtractMetadata(implementationType ?? typeof(TImplementation)))
            {
                AppendRegistryKeys();
            }

            var context = Resolver.CreateRegistryContext(_registrationKeys, new MethodFactory<TImplementation>(factoryMethod), _extensions?.ToArray() ?? EmptyExtensions);
            if (!Resolver.TryRegister(context, out IDisposable registration))
            {
                throw new ContainerException($"Can't register {string.Join(Environment.NewLine, context.Keys.Select(i => i.ToString()).ToArray())}.\nDetails:\n{Resolver}");
            }

            _registrationKeys.Clear();
            _result.AddResource(registration);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private IMethodFactory GetInstanceFactoryProvider()
        {
            if (!Resolver.TryResolve(out IMethodFactory instanceFactoryProvider))
            {
                throw new ContainerException($"{typeof(IMethodFactory)} was not registered.");
            }

            return instanceFactoryProvider;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private IInstanceFactory CreateFactory(Type resolvedType, IMetadataProvider metadataProvider)
        {
            IInstanceFactory factory;
            if (_resolverFactoryCache != null)
            {
                if (_resolverFactoryCache.TryGet(resolvedType, out factory))
                {
                    return factory;
                }

                factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider, Resolver.KeyFactory);
                _resolverFactoryCache.Set(resolvedType, factory);
            }
            else
            {
                factory = new MetadataFactory(resolvedType, _instanceFactoryProvider.Value, metadataProvider, Resolver.KeyFactory);
            }

            return factory;
        }
    }
}