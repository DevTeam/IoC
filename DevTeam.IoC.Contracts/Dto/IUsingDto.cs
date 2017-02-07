namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IUsingDto : IConfigurationStatementDto
    {
        string Using { get; }
    }
}