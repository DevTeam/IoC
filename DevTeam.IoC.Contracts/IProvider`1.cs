namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<TContract>
    {
        bool TryGet(out TContract instance, [NotNull] IStateProvider stateProvider);

        bool TryGet(out TContract instance, [NotNull] params object[] state);
    }
}