namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class TransientLifetime : ILifetime
    {
        public static readonly ILifetime Shared = new TransientLifetime();

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
            return resolverContext.RegistryContext.InstanceFactory.Create(resolverContext);
        }

        public void Dispose()
        {
        }
    }
}