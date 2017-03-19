namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TState1, in TState2, TContract>
    {
        bool TryGet(out TContract instance, [CanBeNull] TState1 state1, [CanBeNull] TState2 state2);
    }
}