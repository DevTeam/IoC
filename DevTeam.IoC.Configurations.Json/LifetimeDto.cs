namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class LifetimeDto : ILifetimeDto
    {
        [JsonProperty("lifetime", Required = Required.Always)]
        public Wellknown.Lifetime Lifetime { get; [NotNull] set; }
    }
}
