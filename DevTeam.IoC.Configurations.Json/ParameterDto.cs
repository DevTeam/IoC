namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ParameterDto : IParameterDto
    {
        [JsonProperty("paramType", Required = Required.Always)]
        public string TypeName { get; [NotNull] set; }

        [JsonProperty("dependency", Required = Required.Default)]
        public IEnumerable<IRegisterStatementDto> Dependency { get; [CanBeNull] set; }

        [JsonProperty("state", Required = Required.Default)]
        public IStateDto State { get; [CanBeNull] set; }

        [JsonProperty("value", Required = Required.Default)]
        public string Value { get; [CanBeNull] set; }
    }
}
