namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class DependencyFeatureDto: IDependencyFeatureDto
    {
        [JsonProperty("dependency.feature", Required = Required.Always)]
        public Wellknown.Feature Feature { get; [NotNull] set; }
    }
}
