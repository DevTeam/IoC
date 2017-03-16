namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IStateProvider
    {
        object Key { [NotNull] get; }

        [CanBeNull]
        object GetState([NotNull] ICreationContext creationContext, [NotNull] IStateKey stateKey);
    }
}
