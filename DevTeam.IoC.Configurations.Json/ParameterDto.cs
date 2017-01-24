﻿namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ParameterDto : IParameterDto
    {
        [JsonProperty("Type", Required = Required.Always)]
        public string TypeName { get; set; }

        [JsonProperty("Dependency", Required = Required.Default)]
        public IEnumerable<IKeyDto> Dependency { get; set; }

        [JsonProperty("State", Required = Required.Default)]
        public IStateDto State { get; set; }

        [JsonProperty("Value", Required = Required.Default)]
        public string Value { get; set; }
    }
}
