namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class PropertyDto : IPropertyDto
    {
        [JsonProperty("name", Required = Required.Default)]
        public string Name { get; [NotNull] set; }

        [JsonProperty("property", Required = Required.Always)]
        public IParameterDto Property { get; [CanBeNull] set; }
    }
}
