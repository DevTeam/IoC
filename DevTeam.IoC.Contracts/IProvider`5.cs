namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TSTate1, in TSTate2, in TSTate3, in TSTate4, TContract>
    {
        bool TryGet(out TContract instance, [CanBeNull] TSTate1 state1, [CanBeNull] TSTate2 state2, [CanBeNull] TSTate3 state3, [CanBeNull] TSTate4 state4);
    }
}