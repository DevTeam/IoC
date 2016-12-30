namespace DevTeam.IoC.Contracts.Dto
{
    public interface IConfigurationReferenceDto: IDependencyDto
    {
        string Reference { get; }

        string ConfigurationTypeName { get; }
    }
}