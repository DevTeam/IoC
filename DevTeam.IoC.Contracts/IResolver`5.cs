namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TSTate1, in TSTate2, in TSTate3, in TSTate4, out TContract>
    {
        TContract Resolve(TSTate1 state1, TSTate2 state2, TSTate3 state3, TSTate4 state4);
    }
}