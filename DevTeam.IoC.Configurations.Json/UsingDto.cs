namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class UsingDto: IUsingDto
    {
        [JsonProperty("using", Required = Required.Always)]
        public string Using { get; set; }
    }
}
