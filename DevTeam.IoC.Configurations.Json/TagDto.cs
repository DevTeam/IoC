namespace DevTeam.IoC.Configurations.Json
{
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class TagDto: ITagDto
    {
        [JsonProperty("tag", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("tagType", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TypeName { get; set; }
    }
}
