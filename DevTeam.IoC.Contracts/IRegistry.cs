namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegistry
    {
        RegistryContext CreateRegistryContext(
            [NotNull] IEnumerable<IKey> keys,
            [NotNull] IInstanceFactory factory,
            [NotNull] params IExtension[] extensions);

        bool TryRegister(RegistryContext context, out IDisposable registration);
    }
}
