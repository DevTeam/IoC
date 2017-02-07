namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IStateProvider
    {
        object GetKey(IResolverContext resolverContext);

        object GetState(IResolverContext resolverContext, IStateKey stateKey);
    }
}
