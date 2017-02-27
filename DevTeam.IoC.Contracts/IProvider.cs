namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider
    {
        bool TryGet(out object instance, [NotNull] IStateProvider stateProvider);

        bool TryGet(out object instance, [NotNull] params object[] state);
    }
}