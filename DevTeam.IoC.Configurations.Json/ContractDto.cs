namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class ContractDto: IContractDto
    {
        [JsonProperty("contract", Required = Required.Always)]
        public IEnumerable<string> Contract { get; [NotNull] set; }
    }
}
