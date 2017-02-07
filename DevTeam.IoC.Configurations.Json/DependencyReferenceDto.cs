namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class DependencyReferenceDto : IDependencyReferenceDto
    {
        [JsonProperty("dependency.reference", Required = Required.Always)]
        public string Reference { get; [NotNull] set; }

        [JsonProperty("referenceType", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ConfigurationTypeName { get; [NotNull] set; }
    }
}
