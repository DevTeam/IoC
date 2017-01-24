namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class TagDto: ITagDto
    {
        [JsonProperty("Tag", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("Type", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TypeName { get; set; }
    }
}
