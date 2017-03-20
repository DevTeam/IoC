namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.GenericParameter | AttributeTargets.Method | AttributeTargets.Property)]

    public class AutowiringAttribute : Attribute
    {
    }
}
