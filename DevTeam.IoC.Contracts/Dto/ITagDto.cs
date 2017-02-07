namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface ITagDto : IRegisterStatementDto
    {
        string Value { get; }

        string TypeName { get; }
    }
}