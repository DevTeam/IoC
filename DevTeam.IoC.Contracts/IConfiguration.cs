namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IConfiguration
    {
        IEnumerable<IConfiguration> GetDependencies(IResolver resolver);

        IEnumerable<IDisposable> Apply(IResolver resolver);
    }
}
