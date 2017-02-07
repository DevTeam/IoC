namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.GenericParameter)]

    public class AutowiringAttribute : Attribute
    {
    }
}
