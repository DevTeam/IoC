namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface IDependencyFeatureDto : IConfigurationStatementDto
    {
        Wellknown.Features Feature { get; }
    }
}