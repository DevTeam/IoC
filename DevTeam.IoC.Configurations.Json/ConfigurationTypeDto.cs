namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ConfigurationTypeDto: IConfigurationTypeDto
    {
        [JsonProperty("TypeName", Required = Required.Always)]
        public string ConfigurationTypeName { get; set; }
    }
}
