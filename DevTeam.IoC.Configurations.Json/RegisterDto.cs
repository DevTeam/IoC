﻿namespace DevTeam.IoC.Configurations.Json

{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class RegisterDto: IRegisterDto
    {
        [JsonProperty("register", Required = Required.Default)]
        public IEnumerable<IRegisterStatementDto> Keys { get; [CanBeNull] set; }

        [JsonProperty("autowiring", Required = Required.Default)]
        public string AutowiringTypeName { get; [CanBeNull] set; }

        [JsonProperty("constructor", Required = Required.Default)]
        public IEnumerable<IParameterDto> ConstructorParameters { get; [CanBeNull] set; }

        [JsonProperty("factoryMethod", Required = Required.Default)]
        public string FactoryMethodName { get; [CanBeNull] set; }
    }
}
