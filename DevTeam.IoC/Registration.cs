﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class Registration<T> : Token<T, IRegistration<T>>, IRegistration<T>
          where T : IContainer
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IExtension[] EmptyExtensions = new IExtension[0];
        private readonly List<HashSet<IContractKey>> _contractKeys = new List<HashSet<IContractKey>>();
        private readonly HashSet<IKey> _registrationKeys = new HashSet<IKey>();
        private readonly Lazy<IMethodFactory> _instanceFactoryProvider;
        private readonly RegistrationResult<T> _result;
        private readonly ICache<Type, IResolverFactory> _resolverFactoryCache;
        private readonly IReflection _reflection;
        [CanBeNull] private HashSet<ITagKey> _tagKeys;
        [CanBeNull] private HashSet<IStateKey> _stateKeys;
        [CanBeNull] private List<IExtension> _extensions;

        internal Registration([NotNull] IFluent fluent, [NotNull] T container)
            : base(container)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (container == null) throw new ArgumentNullException(nameof(container));
            _instanceFactoryProvider = new Lazy<IMethodFactory>(GetInstanceFactoryProvider);
            _result = new RegistrationResult<T>(this);
            var cacheProvider = container as IProvider<ICache<Type, IResolverFactory>>;
            cacheProvider?.TryGet(out _resolverFactoryCache);
            _reflection = container.Resolve().Instance<IReflection>();
        }

        private List<IExtension> Extensions => _extensions ?? (_extensions = new List<IExtension>());

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

        public override IRegistration<T> State(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            AddStateKey(Resolver.KeyFactory.CreateStateKey(index, stateType, false));
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

        public IRegistrationResult<T> FactoryMethod(Func<ICreationContext, object> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            FactoryMethodInternal(factoryMethod);
            return _result;
        }

        public IRegistrationResult<T> FactoryMethod<TImplementation>(Func<ICreationContext, TImplementation> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            FactoryMethodInternal(factoryMethod, typeof(TImplementation));
            return _result;
        }

        public IRegistrationResult<T> Autowiring(Type implementationType, bool lazy = false, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            metadataProvider = metadataProvider ?? Fluent.Resolve(Resolver).Instance<IMetadataProvider>();
            IResolverFactory resolverFactory;
            if (!lazy && metadataProvider.TryResolveType(implementationType, out Type resolvedType))
            {
                resolverFactory = CreateFactory(resolvedType, metadataProvider);
                FactoryMethodInternal(ctx => resolverFactory.Create(ctx), implementationType);
            }
            else
            {
                FactoryMethodInternal(ctx =>
                {
                    if (!metadataProvider.TryResolveType(implementationType, out Type currentResolvedType, ctx))
                    {
                        throw new InvalidOperationException("Can not define type to resolve from type {currentResolvedType}");
                    }

                    return CreateFactory(currentResolvedType, metadataProvider).Create(ctx);
                }, implementationType);
            }

            return _result;
        }

        public IRegistrationResult<T> Autowiring<TImplementation>(bool lazy = false)
        {
            return Autowiring(typeof(TImplementation), lazy);
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

        private void AppendRegistryKeys()
        {
            foreach (var contractKeys in _contractKeys)
            {
                if (contractKeys.Count == 1 && _tagKeys == null && _stateKeys == null)
                {
                    _registrationKeys.Add(contractKeys.Single());
                }
                else
                {
                    _registrationKeys.Add(Resolver.KeyFactory.CreateCompositeKey(contractKeys, _tagKeys, _stateKeys));
                }
            }

            _contractKeys.Clear();
            _stateKeys?.Clear();
            _tagKeys?.Clear();
        }

        private void FactoryMethodInternal<TImplementation>(
            Func<ICreationContext, TImplementation> factoryMethod,
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
                throw new InvalidOperationException($"Can't register {string.Join(Environment.NewLine, context.Keys.Select(i => i.ToString()).ToArray())}.{Environment.NewLine}{Environment.NewLine}{Resolver}");
            }

            _registrationKeys.Clear();
            _result.AddResource(registration);
        }

        private IMethodFactory GetInstanceFactoryProvider()
        {
            if (!Resolver.TryResolve(out IMethodFactory instanceFactoryProvider))
            {
                throw new InvalidOperationException($"{typeof(IMethodFactory)} was not registered.");
            }

            return instanceFactoryProvider;
        }

        private IResolverFactory CreateFactory(Type resolvedType, IMetadataProvider metadataProvider)
        {
            IResolverFactory factory;
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