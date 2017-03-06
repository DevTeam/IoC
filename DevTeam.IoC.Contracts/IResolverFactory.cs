namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolverFactory
    {
        [NotNull]
        object Create([NotNull] ICreationContext creationContext);
    }
}