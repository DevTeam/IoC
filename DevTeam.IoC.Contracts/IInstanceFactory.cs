namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IInstanceFactory
    {
        [NotNull]
        object Create(CreationContext creationContext);
    }
}