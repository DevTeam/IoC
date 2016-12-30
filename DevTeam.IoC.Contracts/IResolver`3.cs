namespace DevTeam.IoC.Contracts
{
    public interface IResolver<in TSTate1, in TSTate2, out TContract>
    {
        TContract Resolve(TSTate1 state1, TSTate2 state2);
    }
}