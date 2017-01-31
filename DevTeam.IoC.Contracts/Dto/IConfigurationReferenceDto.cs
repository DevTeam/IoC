namespace DevTeam.IoC.Contracts.Dto
{
    public interface IConfigurationReferenceDto: IConfigurationStatementDto
    {
        string Reference { get; }

        string ConfigurationTypeName { get; }
    }
}