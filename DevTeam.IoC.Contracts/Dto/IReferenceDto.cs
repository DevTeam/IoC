namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IReferenceDto : IConfigurationStatementDto
    {
        string Reference { get; }
    }
}
