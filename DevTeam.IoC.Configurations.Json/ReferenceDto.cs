namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class ReferenceDto : IReferenceDto
    {
        [JsonProperty("reference", Required = Required.Always)]
        public string Reference { get; [NotNull] set; }
    }
}
