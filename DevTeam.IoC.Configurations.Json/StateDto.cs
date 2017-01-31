namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class StateDto: IStateDto
    {
        [JsonProperty("stateType", Required = Required.Always)]
        public string StateTypeName { get; set; }

        [JsonProperty("index", Required = Required.Default)]
        public int Index { get; set; }

        [JsonProperty("value", Required = Required.Default)]
        public IValueDto Value { get; set; }
    }
}
