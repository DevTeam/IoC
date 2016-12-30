namespace DevTeam.IoC.Contracts
{
    public interface IProvider<in TSTate1, TContract>
    {
        bool TryGet(out TContract instance, TSTate1 state1);
    }
}