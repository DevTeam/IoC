namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<in TSTate1, out TContract>
    {
        [NotNull]
        TContract Resolve([CanBeNull] TSTate1 state1);
    }
}