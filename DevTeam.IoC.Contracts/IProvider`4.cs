namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TState1, in TState2, in TState3, TContract>
    {
        bool TryGet(out TContract instance, [CanBeNull] TState1 state1, [CanBeNull] TState2 state2, [CanBeNull] TState3 state3);
    }
}