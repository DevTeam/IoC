namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal sealed class TransientLifetime : ILifetime
    {
        public static readonly ILifetime Shared = new TransientLifetime();

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public object Create(ILifetimeContext lifetimeContext, ICreationContext creationContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
#if DEBUG
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
#endif
            return creationContext.ResolverContext.RegistryContext.InstanceFactory.Create(creationContext);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose()
        {
        }
    }
}