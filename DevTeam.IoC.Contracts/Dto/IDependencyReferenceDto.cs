namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IDependencyReferenceDto: IConfigurationStatementDto
    {
        string Reference { [NotNull] get; }

        string ConfigurationTypeName { [NotNull] get; }
    }
}