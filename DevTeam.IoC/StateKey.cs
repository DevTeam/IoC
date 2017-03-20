namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal struct StateKey: IStateKey
    {
        [NotNull] private readonly IReflection _reflection;
        private IType _type;

        public StateKey([NotNull] IReflection reflection, int index, [NotNull] Type stateType, bool resolving)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
#endif
            Index = index;
            StateType = stateType;
            Resolving = resolving;
            _reflection = reflection;
            _type = reflection.GetType(StateType);
        }

        public int Index { get; }

        public Type StateType { get; }

        public bool Resolving { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var key = obj as IStateKey ?? (obj as ICompositeKey)?.StateKeys.SingleOrDefault();
            return key != null && Equals(key);
        }

        public override int GetHashCode()
        {
            if (KeyFilterContext.Current.Filter(typeof(IStateKey)))
            {
                return 0;
            }

            return Index;
        }

        private bool Equals(IStateKey other)
        {
            var eq = KeyFilterContext.Current.Filter(typeof(IStateKey)) || Index == other.Index;
            if (!eq)
            {
                return false;
            }

            if (StateType == other.StateType)
            {
                return true;
            }

            var otherType = _reflection.GetType(other.StateType);

            return 
                (other.Resolving && _type.IsAssignableFrom(otherType))
                || (Resolving && otherType.IsAssignableFrom(_type));
        }

        public override string ToString()
        {
            return $"{nameof(StateKey)} [Index: {Index}, StateType: {StateType.Name}, Resolving: {Resolving}]";
        }
    }
}
