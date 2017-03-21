namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal struct StateKey: IStateKey
    {
        [NotNull] private readonly IReflection _reflection;
        private IType _type;
        private readonly int _index;
        private readonly Type _stateType;
        private readonly bool _resolving;

        public StateKey([NotNull] IReflection reflection, int index, [NotNull] Type stateType, bool resolving)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
#endif
            _index = index;
            _stateType = stateType;
            _resolving = resolving;
            _reflection = reflection;
            _type = reflection.GetType(_stateType);
        }

        public int Index => _index;

        public Type StateType => _stateType;

        public bool Resolving => _resolving;

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

            return _index;
        }

        private bool Equals(IStateKey other)
        {
            var eq = KeyFilterContext.Current.Filter(typeof(IStateKey)) || _index == other.Index;
            if (!eq)
            {
                return false;
            }

            if (_stateType == other.StateType)
            {
                return true;
            }

            var otherType = _reflection.GetType(other.StateType);

            return 
                (other.Resolving && _type.IsAssignableFrom(otherType))
                || (_resolving && otherType.IsAssignableFrom(_type));
        }

        public override string ToString()
        {
            return $"{nameof(StateKey)} [Index: {_index}, StateType: {_stateType.Name}, Resolving: {_resolving}]";
        }
    }
}
