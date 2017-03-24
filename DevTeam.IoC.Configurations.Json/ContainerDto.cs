namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class ContainerDto: IContainerDto
    {
        [JsonProperty("container", Required = Required.Always)]
        public IEnumerable<IConfigurationStatementDto> Statements { get; [NotNull] set; }

        [JsonProperty("tag", Required = Required.Default)]
        public ITagDto Tag { get; [NotNull] set; }
    }
}
