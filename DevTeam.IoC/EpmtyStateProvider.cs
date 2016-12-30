namespace DevTeam.IoC
{
    using Contracts;

    internal class EpmtyStateProvider: IStateProvider
    {
        public static readonly IStateProvider Shared = new EpmtyStateProvider();

        public object GetState(IResolverContext resolverContext, IStateKey stateKey)
        {
            throw new System.NotImplementedException();
        }

        public object GetKey(IResolverContext resolverContext)
        {
            return Shared;
        }
    }
}
