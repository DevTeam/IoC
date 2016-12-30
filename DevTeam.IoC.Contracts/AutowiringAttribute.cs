namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor)]

    public class AutowiringAttribute : Attribute
    {
    }
}
