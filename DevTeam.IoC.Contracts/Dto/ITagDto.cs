namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface ITagDto : IRegisterStatementDto
    {
        string Value { [NotNull] get; }

        string TypeName { [CanBeNull] get; }
    }
}