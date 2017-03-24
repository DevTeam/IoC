namespace DevTeam.IoC
{
    using System.Linq;
    using Contracts;

    internal struct ParamsStateProvider: IStateProvider
    {
        private readonly object[] _state;
        private int? _hashCode;

        public static readonly ParamsStateProvider Empty;

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static IStateProvider Create(params object[] state)
        {
            if (state.Length == 0)
            {
                return Empty;
            }

            return new ParamsStateProvider(state);
        }

        private ParamsStateProvider(params object[] state)
        {
            _state = state;
            _hashCode = null;
        }

        public object Key => this;

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public object GetState(ICreationContext creationContext, IStateKey stateKey)
        {
            return _state[stateKey.Index];
        }

        public bool Equals(ParamsStateProvider other)
        {
            return _state.SequenceEqual(other._state);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is ParamsStateProvider))
            {
                return false;
            }

            var stateProvider = (ParamsStateProvider)obj;
            return Equals(stateProvider);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            _hashCode = _hashCode ?? GetHashInternal();
            return _hashCode.Value;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private int GetHashInternal()
        {
            return _state.Aggregate(0, (code, key) =>
            {
                unchecked
                {
                    return (code*397) ^ key?.GetHashCode() ?? 0;
                }
            });
        }
    }
}
