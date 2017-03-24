namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class ValueDto : IValueDto
    {
        [JsonProperty("data", Required = Required.Always)]
        public string Data { get; [NotNull] set; }

        [JsonProperty("valueType", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TypeName { get; [CanBeNull] set; }
    }
}
