namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegistry
    {
        [NotNull]
        IRegistryContext CreateRegistryContext(
            [NotNull] IEnumerable<ICompositeKey> keys,
            [NotNull] IResolverFactory factory,
            [NotNull] IEnumerable<IExtension> extensions);

        bool TryRegister([NotNull] IRegistryContext context, out IDisposable registration);
    }
}
