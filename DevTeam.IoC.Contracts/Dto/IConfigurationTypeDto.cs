namespace DevTeam.IoC.Contracts.Dto
{
    public interface IConfigurationTypeDto : IConfigurationStatementDto
    {
        string ConfigurationTypeName { get; }
    }
}