namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class MethodDto : IMethodDto
    {
        [JsonProperty("name", Required = Required.Default)]
        public string Name { get; [NotNull] set; }

        [JsonProperty("method", Required = Required.Default)]
        public IEnumerable<IParameterDto> MethodParameters { get; [CanBeNull] set; }
    }
}
