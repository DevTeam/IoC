namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IDependencyAssemblyDto : IConfigurationStatementDto
    {
        string AssemblyName { [NotNull] get; set; }
    }
}
