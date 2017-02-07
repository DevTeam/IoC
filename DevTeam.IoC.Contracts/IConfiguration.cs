namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IConfiguration
    {
        [NotNull]
        IEnumerable<IConfiguration> GetDependencies([NotNull] IResolver resolver);

        [NotNull]
        IEnumerable<IDisposable> Apply([NotNull] IResolver resolver);
    }
}
