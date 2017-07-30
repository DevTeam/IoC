namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
#if !NET35
    using System.Threading.Tasks;
#endif
    using Contracts;

    internal sealed class Resolving<TResolver> : Token<TResolver, IResolving<TResolver>>, IResolving<TResolver>
          where TResolver : IResolver
    {
        private readonly HashSet<IContractKey> _сontractKeys = new HashSet<IContractKey>();
        [CanBeNull] private HashSet<ITagKey> _tagKeys;
        [CanBeNull] private HashSet<IStateKey> _stateKeys;
        private IResolverContext _resolverContext;
        private IContractKey _singleContractKey;
        private int _contractKeysCount;

        internal Resolving([NotNull] TResolver resolver)
            : base(resolver)
        {
#if DEBUG
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
#endif
        }

        [NotNull]
        internal IEnumerable<IContractKey> ContractKeys => _сontractKeys;

        [CanBeNull]
        internal IEnumerable<ITagKey> TagKeys => _tagKeys;

        [CanBeNull]
        internal IEnumerable<IStateKey> StateKeys => _stateKeys;

        public override IResolving<TResolver> Contract(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            AddContractKey(contractTypes.Select(type => Resolver.KeyFactory.CreateContractKey(type, true)));
            return this;
        }

        public override IResolving<TResolver> State(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            AddStateKey(Resolver.KeyFactory.CreateStateKey(index, stateType, true));
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
            if (!TryCreateResolverContext(stateProvider, out IKey _, out IResolverContext ctx))
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
            if (_contractKeysCount == 0)
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
            if (_contractKeysCount == 0)
            {
                Contract<TContract>();
            }

#pragma warning disable 168
            if (!TryCreateResolverContext(stateProvider, out IKey _, out IResolverContext ctx))
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
            TrySpecifyState(state);
            return Instance(ParamsStateProvider.Create(state));
        }

        public bool TryInstance(out object instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            TrySpecifyState(state);
            return TryInstance(out instance, ParamsStateProvider.Create(state));
        }

        public bool TryInstance<TContract>(out TContract instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            TrySpecifyState(state);
            return TryInstance(out instance, ParamsStateProvider.Create(state));
        }

        public TContract Instance<TContract>(params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            TrySpecifyState(state);
            return Instance<TContract>(ParamsStateProvider.Create(state));
        }

#if !NET35
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Task AsyncInstance(CancellationToken cancellationToken, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new Task(() => Instance(state));
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Task AsyncInstance(CancellationToken cancellationToken, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            return new Task(() => Instance(stateProvider), cancellationToken);
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Task<TContract> AsyncInstance<TContract>(CancellationToken cancellationToken, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new Task<TContract>(() => Instance<TContract>(state));
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public Task<TContract> AsyncInstance<TContract>(CancellationToken cancellationToken, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            return new Task<TContract>(() => Instance<TContract>(stateProvider));
        }
#endif

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private IKey CreateResolvingKey()
        {
            if (_contractKeysCount == 1 && _tagKeys == null && _stateKeys == null)
            {
                return _singleContractKey;
            }

            return Resolver.KeyFactory.CreateCompositeKey(_сontractKeys, _tagKeys, _stateKeys);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void OnCompositeKeyChanged()
        {
            _resolverContext = null;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private void TrySpecifyState(params object[] state)
        {
            if (_stateKeys != null || state.Length == 0)
            {
                return;
            }

            for (var index = 0; index < state.Length; index++)
            {
                var value = state[index];
                if (value != null)
                {
                    State(index, value.GetType());
                }
                else
                {
                    State<object>(index);
                }
            }
        }
    }
}