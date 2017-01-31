namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class CreateChildDto: ICreateChildDto
    {
        [JsonProperty("CreateChild", Required = Required.Always)]
        public IEnumerable<IConfigurationStatementDto> Statements { get; set; }
    }
}
