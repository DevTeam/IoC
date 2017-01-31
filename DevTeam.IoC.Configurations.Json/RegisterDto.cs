namespace DevTeam.IoC.Configurations.Json

{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class RegisterDto: IRegisterDto
    {
        [JsonProperty("Register", Required = Required.Always)]
        public IEnumerable<IKeyDto> Keys { get; set; }

        [JsonProperty("AsAutowiring", Required = Required.Default)]
        public string AutowiringTypeName { get; set; }

        [JsonProperty("constructor", Required = Required.Default)]
        public IEnumerable<IParameterDto> ConstructorParameters { get; set; }

        [JsonProperty("AsFactoryMethod", Required = Required.Default)]
        public string FactoryMethodName { get; set; }
    }
}
