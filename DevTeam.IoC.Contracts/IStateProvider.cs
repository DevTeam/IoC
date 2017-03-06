namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IStateProvider
    {
        [NotNull]
        object GetKey([NotNull] ICreationContext creationContext);

        [CanBeNull]
        object GetState([NotNull] ICreationContext creationContext, [NotNull] IStateKey stateKey);
    }
}
