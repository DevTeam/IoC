namespace DevTeam.IoC.Contracts.Dto
{
    public interface IConfigurationTypeDto : IDependencyDto
    {
        string ConfigurationTypeName { get; }
    }
}