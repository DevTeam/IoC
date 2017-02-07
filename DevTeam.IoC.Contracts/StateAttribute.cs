namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
    public class StateAttribute : Attribute
    {
        public StateAttribute(int index, Type stateType)
        {
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

        public Type StateType { get; }

        public object Value { get; set; }
    }
}
