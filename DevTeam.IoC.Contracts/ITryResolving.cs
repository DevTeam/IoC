namespace DevTeam.IoC.Contracts
{
    public interface ITryResolving
    {
        bool TryInstance(out object instance, [NotNull][ItemCanBeNull] params object[] state);

        bool TryInstance<TContract>(out TContract instance, [NotNull][ItemCanBeNull] params object[] state);

        bool TryInstance(out object instance, [NotNull] IStateProvider stateProvider);

        bool TryInstance<TContract>(out TContract instance, [NotNull] IStateProvider stateProvider);
    }
}
