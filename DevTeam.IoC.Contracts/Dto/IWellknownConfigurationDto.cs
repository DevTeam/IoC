namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IWellknownConfigurationDto : IDependencyDto
    {
        Wellknown.Configurations Configuration { get; }
    }
}