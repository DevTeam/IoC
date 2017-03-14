﻿namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal struct CreationContext : ICreationContext
    {
        public CreationContext([NotNull] IResolverContext resolverContext, [NotNull] IStateProvider stateProvider)
        {
#if DEBUG
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
#endif
            ResolverContext = resolverContext;
            StateProvider = stateProvider;
        }

        public IResolverContext ResolverContext { get; }

        public IStateProvider StateProvider { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [ ResolverContext: {ResolverContext}, StateProvider: {StateProvider}]";
        }
    }
}