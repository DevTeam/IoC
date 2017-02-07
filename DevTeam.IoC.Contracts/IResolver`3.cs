namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TSTate1, in TSTate2, out TContract>
    {
        [NotNull]
        TContract Resolve([CanBeNull] TSTate1 state1, [CanBeNull] TSTate2 state2);
    }
}