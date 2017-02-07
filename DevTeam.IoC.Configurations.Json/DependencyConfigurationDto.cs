namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class DependencyConfigurationDto: IDependencyConfigurationDto
    {
        [JsonProperty("dependency.configuration", Required = Required.Always)]
        public string ConfigurationTypeName { get; [NotNull] set; }
    }
}
