namespace DevTeam.IoC.Contracts
{
    public interface IResolving : IToken<IResolving>
    {
        object Instance(params object[] state);

        object Instance(IStateProvider stateProvider);

        TContract Instance<TContract>(params object[] state);

        TContract Instance<TContract>(IStateProvider stateProvider);

        bool Instance<TContract>(out TContract instance, params object[] state);

        bool TryInstance(out object instance, params object[] state);

        bool TryInstance(out object instance, IStateProvider stateProvider);

        bool TryInstance<TContract>(out TContract instance, IStateProvider stateProvider);
    }
}