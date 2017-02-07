namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class EmptyStateProvider: IStateProvider
    {
        public static readonly IStateProvider Shared = new EmptyStateProvider();

        public object GetState(IResolverContext resolverContext, IStateKey stateKey)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (stateKey == null) throw new ArgumentNullException(nameof(stateKey));
            return null;
        }

        public object GetKey(IResolverContext resolverContext)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            return Shared;
        }
    }
}
