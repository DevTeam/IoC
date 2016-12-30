namespace DevTeam.IoC.Contracts
{
    public interface IProvider<TContract>
    {
        bool TryGet(out TContract instance, IStateProvider stateProvider);

        bool TryGet(out TContract instance, params object[] state);
    }
}