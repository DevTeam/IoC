namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IDependencyFeatureDto : IConfigurationStatementDto
    {
        Wellknown.Features Feature { get; }
    }
}