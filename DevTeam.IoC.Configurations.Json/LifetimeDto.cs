namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class LifetimeDto : ILifetimeDto
    {
        [JsonProperty("lifetime", Required = Required.Always)]
        public Wellknown.Lifetimes Lifetime { get; [NotNull] set; }
    }
}
