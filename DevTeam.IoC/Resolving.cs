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
        private IResolverContext _resolverContext;
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
            IKey key;
            if (!TryCreateResolverContext(stateProvider, out key, out ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return Resolver.Resolve(ctx, stateProvider);
        }

        public bool TryInstance(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            IResolverContext ctx;
            IKey key;
            if (!TryCreateResolverContext(stateProvider, out key, out ctx))
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
            if (_genericContractKeys.Count == 0)
            {
                Contract<TContract>();
            }

            IResolverContext ctx;
            IKey key;
            if (!TryCreateResolverContext(stateProvider, out key, out ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return (TContract)Resolver.Resolve(ctx, stateProvider);
        }

        public bool TryInstance<TContract>(out TContract instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_genericContractKeys.Count == 0)
            {
                Contract<TContract>();
            }

            IKey key;
            IResolverContext ctx;
            if (!TryCreateResolverContext(stateProvider, out key, out ctx))
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
            var tagKeys = _hasTagKeys ? _tagKeys : null;
            var stateKeys = _hasStateKeys ? _stateKeys : null;
            if (_genericContractKeysCount == 1 && tagKeys == null && stateKeys == null)
            {
                return _singleContractKey;
            }

            return Resolver.KeyFactory.CreateCompositeKey(_genericContractKeys, tagKeys, stateKeys);
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