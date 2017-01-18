﻿namespace DevTeam.IoC.Configurations.Json

{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal class RegisterDto: IRegisterDto
    {
        [JsonProperty("for", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ITagDto TargetTag { get; set; }

        [JsonProperty("Register", Required = Required.Always)]
        public IEnumerable<IKeyDto> Keys { get; set; }

        [JsonProperty("AsAutowiring", Required = Required.Default)]
        public string AutowiringTypeName { get; set; }

        [JsonProperty("Binding", Required = Required.Default)]
        public IBindingDto Binding { get; set; }

        [JsonProperty("AsFactoryMethod", Required = Required.Default)]
        public string FactoryMethodName { get; set; }
    }
}
