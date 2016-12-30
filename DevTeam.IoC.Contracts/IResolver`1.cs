namespace DevTeam.IoC.Contracts
{
    public interface IResolver<out TContract>
    {
        TContract Resolve(IStateProvider stateProvider);

        TContract Resolve(params object[] state);
    }
}