namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IDependencyWellknownDto : IConfigurationStatementDto
    {
        Wellknown.Configurations Configuration { get; }
    }
}