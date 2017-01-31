namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ParameterDto : IParameterDto
    {
        [JsonProperty("paramType", Required = Required.Always)]
        public string TypeName { get; set; }

        [JsonProperty("dependency", Required = Required.Default)]
        public IEnumerable<IKeyDto> Dependency { get; set; }

        [JsonProperty("state", Required = Required.Default)]
        public IStateDto State { get; set; }

        [JsonProperty("value", Required = Required.Default)]
        public string Value { get; set; }
    }
}
