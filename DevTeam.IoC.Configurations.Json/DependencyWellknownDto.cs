namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class DependencyWellknownDto: IDependencyWellknownDto
    {
        [JsonProperty("dependency.wellknown", Required = Required.Always)]
        public Wellknown.Configurations Configuration { get; set; }
    }
}
