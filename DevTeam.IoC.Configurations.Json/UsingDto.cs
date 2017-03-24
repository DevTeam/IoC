namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class UsingDto: IUsingDto
    {
        [JsonProperty("using", Required = Required.Always)]
        public string Using { get; [NotNull] set; }
    }
}
