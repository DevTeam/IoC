namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal struct StateKey: IStateKey
    {
        public StateKey(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

            Index = index;
            StateType = stateType;
        }

        public int Index { get; }

        public Type StateType { get; }

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

            unchecked
            {
                return (Index * 397) ^ StateType.GetHashCode();
            }
        }

        private bool Equals(IStateKey other)
        {
            return KeyFilterContext.Current.Filter(typeof(IStateKey)) || Index == other.Index && StateType == other.StateType;
        }

        public override string ToString()
        {
            return $"{nameof(StateKey)} [Index: {Index}, StateType: {StateType.Name}]";
        }
    }
}
