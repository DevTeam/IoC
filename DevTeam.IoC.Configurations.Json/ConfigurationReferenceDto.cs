namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ConfigurationReferenceDto : IConfigurationReferenceDto
    {
        [JsonProperty("Reference", Required = Required.Always)]
        public string Reference { get; set; }

        [JsonProperty("Type", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ConfigurationTypeName { get; set; }
    }
}
