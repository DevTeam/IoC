namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TSTate1, TContract>
    {
        bool TryGet(out TContract instance, TSTate1 state1);
    }
}