namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class Resolving<T> : Token<T, IResolving<T>>, IResolving<T>
          where T : IResolver
    {
        private readonly HashSet<IContractKey> _genericContractKeys = new HashSet<IContractKey>();
        private readonly HashSet<ITagKey> _tagKeys = new HashSet<ITagKey>();
        private readonly HashSet<IStateKey> _stateKeys = new HashSet<IStateKey>();
        private IKey _resolvingKey;
        private bool _hasTagKeys;
        private bool _hasStateKeys;
        private IContractKey _singleContractKey;
        private int _genericContractKeysCount;

        public Resolving([NotNull] T container)
            : base(container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
        }

        public override IResolving<T> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => Resolver.KeyFactory.CreateContractKey(type, true)));
            return this;
        }

        public object Instance(IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            IResolverContext ctx;
            var key = CreateResolvingKey();
            if (!Resolver.TryCreateResolverContext(key, out ctx, stateProvider))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return Resolver.Resolve(ctx);
        }

        public bool TryInstance(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            IResolverContext ctx;
            var key = CreateResolvingKey();
            if (!Resolver.TryCreateResolverContext(key, out ctx, stateProvider))
            {
                instance = default(object);
                return false;
            }

            instance = Resolver.Resolve(ctx);
            return true;
        }

        public TContract Instance<TContract>(IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_genericContractKeys.Count == 0)
            {
                Contract<TContract>();
            }

            IResolverContext ctx;
            var key = CreateResolvingKey();
            if (!Resolver.TryCreateResolverContext(key, out ctx, stateProvider))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return (TContract)Resolver.Resolve(ctx);
        }

        public bool TryInstance<TContract>(out TContract instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_genericContractKeys.Count == 0)
            {
                Contract<TContract>();
            }

            IResolverContext ctx;
            var key = CreateResolvingKey();
            if (!Resolver.TryCreateResolverContext(key, out ctx, stateProvider))
            {
                instance = default(TContract);
                return false;
            }

            instance = (TContract)Resolver.Resolve(ctx);
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
                changed |= _genericContractKeys.Add(contractKey);
                if (changed)
                {
                    _genericContractKeysCount++;
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
            if (_stateKeys.Add(key))
            {
                _hasStateKeys = true;
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        protected override bool AddTagKey(ITagKey key)
        {
            if (_tagKeys.Add(key))
            {
                _hasTagKeys = true;
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

        internal IKey CreateResolvingKey()
        {
            if (_resolvingKey != null)
            {
                return _resolvingKey;
            }

            var tagKeys = _hasTagKeys ? _tagKeys : null;
            var stateKeys = _hasStateKeys ? _stateKeys : null;
            if (_genericContractKeysCount == 1 && tagKeys == null && stateKeys == null)
            {
                return _singleContractKey;
            }

            _resolvingKey = Resolver.KeyFactory.CreateCompositeKey(_genericContractKeys, tagKeys, stateKeys);
            return _resolvingKey;
        }

        private void OnCompositeKeyChanged()
        {
            _resolvingKey = null;
        }

        private string GetCantResolveErrorMessage(IKey key)
        {
            return $"Can't resolve {key}.{Environment.NewLine}{Environment.NewLine}{Resolver}";
        }
    }
}