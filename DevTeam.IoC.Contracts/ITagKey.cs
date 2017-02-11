namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface ITagKey: IKey
    {
        object Tag { [NotNull] get; }
    }
}
