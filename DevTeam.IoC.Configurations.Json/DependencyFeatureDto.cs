namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class DependencyFeatureDto: IDependencyFeatureDto
    {
        [JsonProperty("dependency.feature", Required = Required.Always)]
        public Wellknown.Features Feature { get; [NotNull] set; }
    }
}
