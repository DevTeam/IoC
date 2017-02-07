namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IDependencyConfigurationDto : IConfigurationStatementDto
    {
        string ConfigurationTypeName { [NotNull] get; }
    }
}