﻿namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface ILifetime: IExtension, IDisposable
    {
        [NotNull]
        object Create([NotNull] ILifetimeContext lifetimeContext, [NotNull] IResolverContext resolverContext, [NotNull] IEnumerator<ILifetime> lifetimeEnumerator);
    }
}
