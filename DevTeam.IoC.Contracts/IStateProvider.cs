namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IStateProvider
    {
        object Key { [NotNull] get; }

        [CanBeNull]
        object GetState(CreationContext creationContext, [NotNull] IStateKey stateKey);
    }
}
