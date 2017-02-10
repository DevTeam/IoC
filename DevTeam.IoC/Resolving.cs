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
        private ICompositeKey _compositeKey;

        public Resolving([NotNull] IFluent fluent, [NotNull] T resolver)
            : base(fluent, resolver)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
        }

        public override IResolving<T> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => KeyFactory.CreateContractKey(type, true)));
            return this;
        }

        public object Instance(IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            IResolverContext ctx;
            var key = CreateCompositeKey();
            if (!TryCreateContext(Resolver, key, stateProvider, out ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return Resolver.Resolve(ctx);
        }

        public bool TryInstance(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            IResolverContext ctx;
            var key = CreateCompositeKey();
            if (!TryCreateContext(Resolver, key, stateProvider, out ctx))
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
            Contract<TContract>();
            IResolverContext ctx;
            var key = CreateCompositeKey();
            if (!TryCreateContext(Resolver, key, stateProvider, out ctx))
            {
                throw new InvalidOperationException(GetCantResolveErrorMessage(key));
            }

            return (TContract)Resolver.Resolve(ctx);
        }

        public bool TryInstance<TContract>(out TContract instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            Contract<TContract>();
            IResolverContext ctx;
            var key = CreateCompositeKey();
            if (!TryCreateContext(Resolver, key, stateProvider, out ctx))
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
            return Instance(new ParamsStateProvider(state));
        }

        public bool TryInstance(out object instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return TryInstance(out instance, new ParamsStateProvider(state));
        }

        public TContract Instance<TContract>(params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return Instance<TContract>(new ParamsStateProvider(state));
        }

        public bool Instance<TContract>(out TContract instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return TryInstance(out instance, new ParamsStateProvider(state));
        }

        protected override bool AddContractKey(IEnumerable<IContractKey> keys)
        {
            var changed = false;
            foreach (var contractKey in keys)
            {
                changed |= _genericContractKeys.Add(contractKey);
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
                OnCompositeKeyChanged();
                return true;
            }

            return false;
        }

        protected override bool AddTagKey(ITagKey key)
        {
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

        internal ICompositeKey CreateCompositeKey()
        {
            if (_compositeKey == null)
            {
                _compositeKey = KeyFactory.CreateCompositeKey(_genericContractKeys, _tagKeys, _stateKeys);
            }

            return _compositeKey;
        }

        private void OnCompositeKeyChanged()
        {
            _compositeKey = null;
        }

        private bool TryCreateContext(IResolver resolver, ICompositeKey key, IStateProvider stateProvider, out IResolverContext resolverContext)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));

            return resolver.TryCreateContext(key, out resolverContext, stateProvider);
        }

        private string GetCantResolveErrorMessage(ICompositeKey key)
        {
            return $"Can't resolve {key}.{Environment.NewLine}{Environment.NewLine}{Resolver}";
        }
    }
}