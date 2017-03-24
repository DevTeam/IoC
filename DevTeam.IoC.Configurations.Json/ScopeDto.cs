namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class ScopeDto: IScopeDto
    {
        [JsonProperty("scope", Required = Required.Always)]
        public Wellknown.Scope Scope { get; [NotNull] set; }
    }
}
