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
                    typeof(ContainerDto),
                    typeof(DependencyReferenceDto),
                    typeof(DependencyConfigurationDto),
                    typeof(DependencyFeatureDto)),
                new JsonEnumConverter<Wellknown.Features>(),
                new JsonEnumConverter<Wellknown.Lifetimes>(),
                new JsonEnumConverter<Wellknown.Scopes>(),
                new JsonEnumConverter<Wellknown.KeyComparers>(),
                new JsonDerivedTypeConverter<IRegisterStatementDto>(
                    typeof(TagDto),
                    typeof(ContractDto),
                    typeof(ScopeDto),
                    typeof(LifetimeDto),
                    typeof(KeyComparerDto),
                    typeof(StateDto)),
                new JsonDerivedTypeConverter<ITagDto>(
                    typeof(TagDto)),
                new JsonDerivedTypeConverter<IParameterDto>(typeof(ParameterDto)),
                new JsonDerivedTypeConverter<IValueDto>(typeof(ValueDto)),
                new JsonDerivedTypeConverter<IStateDto>(typeof(StateDto))
            }
        };

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver.Feature(Wellknown.Features.Dto);
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver
                .Register()
                .Tag(GetType())
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfigurationDto>()
                .AsFactoryMethod(ctx =>
                {
                    var configurationDescriptionDto = ctx.GetState<IConfigurationDescriptionDto>(0);
                    if (configurationDescriptionDto == null)
                    {
                        throw new InvalidOperationException($"{nameof(configurationDescriptionDto)} should not be null.");
                    }

                    return JsonConvert.DeserializeObject<ConfigurationDto>(configurationDescriptionDto.Description, SerializerSettings);
                });
        }
    }
}
