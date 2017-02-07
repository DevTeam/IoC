namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class KeyComparerDto : IKeyComparerDto
    {
        [JsonProperty("keyComparer", Required = Required.Always)]
        public Wellknown.KeyComparers KeyComparer { get; [NotNull] set; }
    }
}
