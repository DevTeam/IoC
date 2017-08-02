namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class ClassDto: RegisterDto
    {
        private string _class;
        private string _autowiringTypeName;
        private List<IRegisterStatementDto> _keys = new List<IRegisterStatementDto>();
        
        [JsonProperty("class", Required = Required.Always)]
        public string Class
        {
            get => _class;

            set
            {
                _keys.Clear();
                _autowiringTypeName = null;
                var parts = value.Split(':');
                if (parts.Length != 1 && parts.Length != 2)
                {
                    ThrowCommonException(value);
                }

                _autowiringTypeName = parts[0].Trim();
                if (_autowiringTypeName == string.Empty)
                {
                    throw new ContainerException($"Invalid \"{nameof(AutowiringTypeName)}\" in the class defenition {value}");
                }

                if (parts.Length > 1)
                {
                    var contracts = parts[1].Trim().Split(',').Select(i => i.Trim()).Where(i => i != string.Empty).ToArray();
                    if (!contracts.Any())
                    {
                        ThrowCommonException(value);
                    }

                    foreach (var contract in contracts)
                    {
                        _keys.Add(new ContractDto { Contract = new []{ contract } });
                    }
                }
                else
                {
                    _keys.Add(new ContractDto { Contract = Enumerable.Repeat(_autowiringTypeName, 1) });
                }

                _class = value;
            }
        }

        [JsonIgnore]
        public override string AutowiringTypeName
        {
            get => _autowiringTypeName ?? base.AutowiringTypeName;
            set => base.AutowiringTypeName = value;
        }

        [JsonProperty("configure", Required = Required.Default)]
        public override IEnumerable<IRegisterStatementDto> Keys
        {
            get => (base.Keys ?? Enumerable.Empty<IRegisterStatementDto>()).Concat(_keys ?? Enumerable.Empty<IRegisterStatementDto>()).ToArray();
            set => base.Keys = value;
        }

        private void ThrowCommonException(string value)
        {
            throw new ContainerException($"Invalid class defenition {value}. Should be \"class_name: interface_name\"");
        }
    }
}
