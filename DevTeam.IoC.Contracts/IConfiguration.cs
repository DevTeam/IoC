namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IConfiguration
    {
        [NotNull]
        IEnumerable<IConfiguration> GetDependencies([NotNull] IContainer container);

        [NotNull]
        IEnumerable<IDisposable> Apply([NotNull] IContainer container);
    }
}
