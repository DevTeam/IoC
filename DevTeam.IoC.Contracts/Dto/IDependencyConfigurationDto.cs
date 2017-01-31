namespace DevTeam.IoC.Contracts.Dto
{
    public interface IDependencyConfigurationDto : IConfigurationStatementDto
    {
        string ConfigurationTypeName { get; }
    }
}