namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class BindingDto : IBindingDto
    {
        [JsonProperty("Constructor", Required = Required.Default)]
        public IEnumerable<IParameterDto> ConstructorParameters { get; set; }
    }
}
