namespace DevTeam.IoC.Contracts.Dto
{
    public interface IDependencyTypeDto : IConfigurationStatementDto
    {
        string ConfigurationTypeName { get; }
    }
}