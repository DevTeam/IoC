namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolverFactory
    {
        object Create(IResolverContext resolverContext);
    }
}