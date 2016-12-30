namespace DevTeam.IoC
{
    using System.Collections.Generic;

    using Contracts;

    internal class TransientLifetime : ILifetime
    {
        public static readonly ILifetime Shared = new TransientLifetime();

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            return resolverContext.RegistryContext.InstanceFactory.Create(resolverContext);
        }

        public void Dispose()
        {
        }
    }
}