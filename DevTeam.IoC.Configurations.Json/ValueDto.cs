namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ValueDto : IValueDto
    {
        [JsonProperty("data", Required = Required.Always)]
        public string Data { get; set; }

        [JsonProperty("valueType", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TypeName { get; set; }
    }
}
