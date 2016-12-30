namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ReferenceDto : IReferenceDto
    {
        [JsonProperty("reference", Required = Required.Always)]
        public string Reference { get; set; }
    }
}
