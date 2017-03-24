namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class TagDto: ITagDto
    {
        [JsonProperty("tag", Required = Required.Always)]
        public string Value { get; [NotNull] set; }

        [JsonProperty("tagType", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TypeName { get; [CanBeNull] set; }
    }
}
