namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class StateAttribute : Attribute
    {
        public StateAttribute(int index, Type stateType)
        {
            Index = index;
            StateType = stateType;
        }

        public int Index { get; }

        public Type StateType { get; }

        public object Value { get; set; }
    }
}
