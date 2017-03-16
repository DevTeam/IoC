namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class Resolving<T> : Token<T, IResolving<T>>, IResolving<T>
          where T : IResolver
    {
        private readonly HashSet<IContractKey> _сontractKeys = new HashSet<IContractKey>();
        [CanBeNull] private HashSet<ITagKey> _tagKeys;
        [CanBeNull] private HashSet<IStateKey> _stateKeys;
        private IResolverContext _resolverContext;
        private IContractKey _singleContractKey;
        private int _contractKeysCount;

        internal Resolving([NotNull] T container)
            : base(container)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
#endif
        }

        [NotNull]
        internal IEnumerable<IContractKey> ContractKeys => _сontractKeys;

        [CanBeNull]
        internal IEnumerable<ITagKey> TagKeys => _tagKeys;

        [CanBeNull]
        internal IEnumerable<IStateKey> StateKeys => _stateKeys;

        public override IResolving<T> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => Resolver.KeyFactory.CreateContractKey(type, true)));
            return this;
        }

        public object Instance(IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (!TryCreateResolverContext(stateProvider, out IKey key, out IResolverContext ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return Resolver.Resolve(ctx, stateProvider);
        }

        public bool TryInstance(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
#pragma warning disable 168
            if (!TryCreateResolverContext(stateProvider, out IKey key, out IResolverContext ctx))
#pragma warning restore 168
            {
                instance = default(object);
                return false;
            }

            instance = Resolver.Resolve(ctx, stateProvider);
            return true;
        }

        public TContract Instance<TContract>(IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_сontractKeys.Count == 0)
            {
                Contract<TContract>();
            }

            if (!TryCreateResolverContext(stateProvider, out IKey key, out IResolverContext ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return (TContract)Resolver.Resolve(ctx, stateProvider);
        }

        public bool TryInstance<TContract>(out TContract instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_сontractKeys.Count == 0)
            {
                Contract<TContract>();
            }

#pragma warning disable 168
            if (!TryCreateResolverContext(stateProvider, out IKey key, out IResolverContext ctx))
#pragma warning restore 168
            {
                instance = default(TContract);
                return false;
            }

            instance = (TContract)Resolver.Resolve(ctx, stateProvider);
            return true;
        }

        public object Instance(params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return Instance(ParamsStateProvider.Create(state));
        }

        public bool TryInstance(out object instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return TryInstance(out instance, ParamsStateProvider.Create(state));
        }

        public bool TryInstance<TContract>(out TContract instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return TryInstance(out instance, ParamsStateProvider.Create(state));
        }

        public TContract Instance<TContract>(params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return Instance<TContract>(ParamsStateProvider.Create(state));
        }

        public bool Instance<TContract>(out TContract instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return TryInstance(out instance, ParamsStateProvider.Create(state));
        }

        protected override bool AddContractKey(IEnumerable<IContractKey> keys)
        {
            var changed = false;
            foreach (var contractKey in keys)
            {
                changed |= _сontractKeys.Add(contractKey);
                if (changed)
                {
                    _contractKeysCount++;
                    _singleContractKey = contractKey;
                }
            }

            if (changed)
            {
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        protected override bool AddStateKey(IStateKey key)
        {
            if (_stateKeys == null)
            {
                _stateKeys = new HashSet<IStateKey>();
            }

            if (_stateKeys.Add(key))
            {
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        protected override bool AddTagKey(ITagKey key)
        {
            if (_tagKeys == null)
            {
                _tagKeys = new HashSet<ITagKey>();
            }

            if (_tagKeys.Add(key))
            {
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        protected override bool AddCompositeKey(ICompositeKey compositeKey)
        {
            var changed = false;
            changed |= AddContractKey(compositeKey.ContractKeys);
            foreach (var tagKey in compositeKey.TagKeys)
            {
                changed |= AddTagKey(tagKey);
            }

            foreach (var stateKey in compositeKey.StateKeys)
            {
                changed |= AddStateKey(stateKey);
            }

            if (changed)
            {
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        private IKey CreateResolvingKey()
        {
            if (_contractKeysCount == 1 && _tagKeys == null && _stateKeys == null)
            {
                return _singleContractKey;
            }

            return Resolver.KeyFactory.CreateCompositeKey(_сontractKeys, _tagKeys, _stateKeys);
        }

        private void OnCompositeKeyChanged()
        {
            _resolverContext = null;
        }

        private bool TryCreateResolverContext(
            IStateProvider stateProvider,
            out IKey key,
            out IResolverContext ctx)
        {
            if (_resolverContext != null)
            {
                key = _resolverContext.Key;
                ctx = _resolverContext;
                return true;
            }

            key = CreateResolvingKey();
            if (!Resolver.TryCreateResolverContext(key, out ctx))
            {
                return false;
            }

            _resolverContext = ctx;
            return true;
        }


        private string GetCantResolveErrorMessage(IKey key)
        {
            return $"Can't resolve {key}.{Environment.NewLine}{Environment.NewLine}{Resolver}";
        }
    }
}