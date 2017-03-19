namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TState1, TContract>
    {
        bool TryGet(out TContract instance, [CanBeNull] TState1 state1);
    }
}