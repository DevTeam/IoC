namespace DevTeam.IoC.Contracts.Dto
{
    public interface ITagDto : IRegisterStatementDto
    {
        string Value { get; }

        string TypeName { get; }
    }
}