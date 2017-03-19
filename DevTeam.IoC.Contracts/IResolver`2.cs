namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TState1, out TContract>
    {
        [NotNull]
        TContract Resolve([CanBeNull] TState1 state1);
    }
}