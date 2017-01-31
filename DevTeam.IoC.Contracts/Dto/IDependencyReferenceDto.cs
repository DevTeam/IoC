namespace DevTeam.IoC.Contracts.Dto
{
    public interface IDependencyReferenceDto: IConfigurationStatementDto
    {
        string Reference { get; }

        string ConfigurationTypeName { get; }
    }
}