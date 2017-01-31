namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    public class JsonConfiguration: IConfiguration
    {
        internal static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new JsonDerivedTypeConverter<IConfigurationStatementDto>(
                    typeof(ReferenceDto),
                    typeof(UsingDto),
                    typeof(RegisterDto),
                    typeof(CreateChildDto),
                    typeof(DependencyReferenceDto),
                    typeof(DependencyTypeDto),
                    typeof(DependencyWellknownDto)),
                new JsonEnumConverter<Wellknown.Configurations>(),
                new JsonEnumConverter<Wellknown.Lifetimes>(),
                new JsonEnumConverter<Wellknown.Scopes>(),
                new JsonEnumConverter<Wellknown.KeyComparers>(),
                new JsonDerivedTypeConverter<IKeyDto>(
                    typeof(TagDto),
                    typeof(ContractDto),
                    typeof(ScopeDto),
                    typeof(LifetimeDto),
                    typeof(KeyComparerDto),
                    typeof(StateDto)),
                new JsonDerivedTypeConverter<IParameterDto>(typeof(ParameterDto)),
                new JsonDerivedTypeConverter<IValueDto>(typeof(ValueDto)),
                new JsonDerivedTypeConverter<IStateDto>(typeof(StateDto)),
            }
        };

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return resolver.Configuration(Wellknown.Configurations.Dto);
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield return resolver
                .Register()
                .Tag(GetType())
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfigurationDto>()
                .AsFactoryMethod(ctx =>
                {
                    var configurationDescriptionDto = ctx.GetState<IConfigurationDescriptionDto>(0);
                    return JsonConvert.DeserializeObject<ConfigurationDto>(configurationDescriptionDto.Description, SerializerSettings);
                });
        }
    }
}
