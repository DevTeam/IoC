namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IConfiguration
    {
        [NotNull]
        IEnumerable<IConfiguration> GetDependencies<T>([NotNull] T container) where T : IContainer;

        [NotNull]
        IEnumerable<IDisposable> Apply<T>([NotNull] T container) where T : IContainer;
    }
}
