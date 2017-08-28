namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IInstanceResolving
    {
        [NotNull]
        object Instance([NotNull][ItemCanBeNull] params object[] state);

        [NotNull]
        object Instance([NotNull] IStateProvider stateProvider);

        [NotNull]
        TContract Instance<TContract>([NotNull][ItemCanBeNull] params object[] state);

        [NotNull]
        TContract Instance<TContract>([NotNull] IStateProvider stateProvider);
    }
}
