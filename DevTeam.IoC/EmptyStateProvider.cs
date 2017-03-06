namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class EmptyStateProvider: IStateProvider
    {
        public static readonly IStateProvider Shared = new EmptyStateProvider();

        private EmptyStateProvider()
        {
        }

        public object GetState(ICreationContext creationContext, IStateKey stateKey)
        {
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            if (stateKey == null) throw new ArgumentNullException(nameof(stateKey));
            return null;
        }

        public object GetKey(ICreationContext creationContext)
        {
            return Shared;
        }
    }
}
