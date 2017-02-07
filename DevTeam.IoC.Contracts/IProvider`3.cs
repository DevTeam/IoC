namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<in TSTate1, in TSTate2, TContract>
    {
        bool TryGet(out TContract instance, [CanBeNull] TSTate1 state1, [CanBeNull] TSTate2 state2);
    }
}