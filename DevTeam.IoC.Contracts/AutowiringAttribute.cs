namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.GenericParameter | AttributeTargets.Method | AttributeTargets.Property)]

    public class AutowiringAttribute : Attribute
    {
        [CanBeNull] public IComparable Order { get; set; }
    }
}
