namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IDependencyReferenceDto: IConfigurationStatementDto
    {
        string Reference { get; }

        string ConfigurationTypeName { get; }
    }
}