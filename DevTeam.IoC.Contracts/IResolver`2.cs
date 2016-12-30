namespace DevTeam.IoC.Contracts
{
    public interface IResolver<in TSTate1, out TContract>
    {
        TContract Resolve(TSTate1 state1);
    }
}