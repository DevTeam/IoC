namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class WellknownConfigurationDto: IWellknownConfigurationDto
    {
        [JsonProperty("Wellknown", Required = Required.Always)]
        public Wellknown.Configurations Configuration { get; set; }
    }
}
