namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IConfiguration
    {
        [NotNull]
        IEnumerable<IConfiguration> GetDependencies<T>([NotNull] T resolver) where T : IResolver;

        [NotNull]
        IEnumerable<IDisposable> Apply([NotNull] IResolver resolver);
    }
}
