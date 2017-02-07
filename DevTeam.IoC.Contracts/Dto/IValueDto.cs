namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IValueDto
    {
        string Data { [NotNull] get; }

        string TypeName { [CanBeNull] get; }
    }
}
