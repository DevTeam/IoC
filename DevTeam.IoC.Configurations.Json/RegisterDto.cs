namespace DevTeam.IoC.Configurations.Json

{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class RegisterDto: IRegisterDto
    {
        [JsonProperty("Register", Required = Required.Default)]
        public IEnumerable<IRegisterStatementDto> Keys { get; [CanBeNull] set; }

        [JsonProperty("AsAutowiring", Required = Required.Default)]
        public string AutowiringTypeName { get; [CanBeNull] set; }

        [JsonProperty("constructor", Required = Required.Default)]
        public IEnumerable<IParameterDto> ConstructorParameters { get; [CanBeNull] set; }

        [JsonProperty("AsFactoryMethod", Required = Required.Default)]
        public string FactoryMethodName { get; [CanBeNull] set; }
    }
}
