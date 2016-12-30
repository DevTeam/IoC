namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class GetDependeciesDto: IGetDependeciesDto
    {
        [JsonProperty("GetDependecies", Required = Required.Always)]
        public IEnumerable<IDependencyDto> Dependecies { get; set; }
    }
}
