namespace DevTeam.IoC.Contracts
{
    public interface IStateProvider
    {
        object GetKey(IResolverContext resolverContext);

        object GetState(IResolverContext resolverContext, IStateKey stateKey);
    }
}
