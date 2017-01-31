namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IWellknownConfigurationDto : IConfigurationStatementDto
    {
        Wellknown.Configurations Configuration { get; }
    }
}