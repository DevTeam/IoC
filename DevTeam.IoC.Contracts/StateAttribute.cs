namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
    public class StateAttribute : Attribute
    {
        public StateAttribute(int index, [NotNull] Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            StateType = stateType;
            IsDependency = true;
        }

        public StateAttribute()
        {
            StateType = typeof(object);
        }

        public bool IsDependency { get; private set; }

        public int Index { get; }

        public Type StateType { [NotNull] get; }

        [CanBeNull]
        public object Value { get; set; }
    }
}
