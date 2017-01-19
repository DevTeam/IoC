namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class StateDto: IStateDto
    {
        [JsonProperty("State", Required = Required.Always)]
        public int Index { get; set; }

        [JsonProperty("TypeName", Required = Required.Always)]
        public string TypeName { get; set; }

        [JsonProperty("Value", Required = Required.Default)]
        public IValueDto Value { get; set; }
    }
}
