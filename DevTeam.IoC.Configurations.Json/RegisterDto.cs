namespace DevTeam.IoC.Configurations.Json

{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class RegisterDto: IRegisterDto
    {
        [JsonProperty("configure", Required = Required.Default)]
        public virtual IEnumerable<IRegisterStatementDto> Keys { get; [CanBeNull] set; }

        [JsonProperty("autowiring", Required = Required.Default)]
        public virtual string AutowiringTypeName { get; [CanBeNull] set; }

        [JsonProperty("constructor", Required = Required.Default)]
        public virtual IEnumerable<IParameterDto> ConstructorParameters { get; [CanBeNull] set; }

        [JsonProperty("methods", Required = Required.Default)]
        public virtual IEnumerable<IMethodDto> Methods { get; [CanBeNull] set; }

        [JsonProperty("properties", Required = Required.Default)]
        public virtual IEnumerable<IPropertyDto> Properties { get; [CanBeNull] set; }

        [JsonProperty("factoryMethod", Required = Required.Default)]
        public virtual string FactoryMethodName { get; [CanBeNull] set; }
    }
}
