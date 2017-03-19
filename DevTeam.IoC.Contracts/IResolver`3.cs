namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TState1, in TState2, out TContract>
    {
        [NotNull]
        TContract Resolve([CanBeNull] TState1 state1, [CanBeNull] TState2 state2);
    }
}