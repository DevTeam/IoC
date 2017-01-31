namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ScopeDto: IScopeDto
    {
        [JsonProperty("scope", Required = Required.Always)]
        public Wellknown.Scopes Scope { get; set; }
    }
}
