namespace DevTeam.IoC.Contracts
{
    public interface IResolverFactory
    {
        object Create(IResolverContext resolverContext);
    }
}