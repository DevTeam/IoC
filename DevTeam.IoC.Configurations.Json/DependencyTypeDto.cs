namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class DependencyTypeDto: IDependencyTypeDto
    {
        [JsonProperty("dependency.type", Required = Required.Always)]
        public string ConfigurationTypeName { get; set; }
    }
}
