namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ApplyDto: IApplyDto
    {
        [JsonProperty("Apply", Required = Required.Always)]
        public IEnumerable<IApplyStatementDto> Statements { get; set; }
    }
}
