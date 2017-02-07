namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TSTate1, in TSTate2, in TSTate3, in TSTate4, out TContract>
    {
        [NotNull]
        TContract Resolve([CanBeNull] TSTate1 state1, [CanBeNull] TSTate2 state2, [CanBeNull] TSTate3 state3, [CanBeNull] TSTate4 state4);
    }
}