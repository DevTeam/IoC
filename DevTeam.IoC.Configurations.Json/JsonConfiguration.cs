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
                    typeof(DependencyAssemblyDto),
                    typeof(DependencyFeatureDto)),
                new JsonEnumConverter<Wellknown.Feature>(),
                new JsonEnumConverter<Wellknown.Lifetime>(),
                new JsonEnumConverter<Wellknown.Scope>(),
                new JsonEnumConverter<Wellknown.KeyComparer>(),
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

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Dto);
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container
                .Register()
                .Tag(GetType())
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfigurationDto>()
                .FactoryMethod(ctx => JsonConvert.DeserializeObject<ConfigurationDto>(ctx.GetState<IConfigurationDescriptionDto>(0).Description, SerializerSettings));
        }
    }
}
