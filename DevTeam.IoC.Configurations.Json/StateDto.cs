namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class StateDto: IStateDto
    {
        [JsonProperty("Index", Required = Required.Default)]
        public int Index { get; set; }

        [JsonProperty("Type", Required = Required.Always)]
        public string TypeName { get; set; }

        [JsonProperty("Value", Required = Required.Default)]
        public IValueDto Value { get; set; }
    }
}
