namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IRegistry
    {
        IRegistryContext CreateContext(
            IEnumerable<ICompositeKey> keys,
            IResolverFactory factory,
            IEnumerable<IExtension> extensions);

        bool TryRegister(IRegistryContext context, out IDisposable registration);
    }
}
