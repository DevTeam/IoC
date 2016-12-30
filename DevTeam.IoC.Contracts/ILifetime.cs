namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface ILifetime: IExtension, IDisposable
    {
        object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator);
    }
}
