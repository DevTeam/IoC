namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    public class JsonConfiguration: IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Dto);
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();

            yield return container
                .Register()
                .Tag(GetType())
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfigurationDto>()
                .FactoryMethod(ctx => JsonConvert.DeserializeObject<ConfigurationDto>(ctx.GetState<IConfigurationDescriptionDto>(0).Description, CreateSerializerSettings(reflection)));
        }

        internal static JsonSerializerSettings CreateSerializerSettings(IReflection reflection)
        {
            return new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new JsonDerivedTypeConverter<IConfigurationStatementDto>(
                        reflection,
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
                        reflection,
                        typeof(TagDto),
                        typeof(ContractDto),
                        typeof(ScopeDto),
                        typeof(LifetimeDto),
                        typeof(KeyComparerDto),
                        typeof(StateDto)),
                    new JsonDerivedTypeConverter<ITagDto>(reflection, typeof(TagDto)),
                    new JsonDerivedTypeConverter<IParameterDto>(reflection, typeof(ParameterDto)),
                    new JsonDerivedTypeConverter<IValueDto>(reflection, typeof(ValueDto)),
                    new JsonDerivedTypeConverter<IStateDto>(reflection, typeof(StateDto)),
                    new JsonDerivedTypeConverter<IMethodDto>(reflection, typeof(MethodDto)),
                    new JsonDerivedTypeConverter<IPropertyDto>(reflection, typeof(PropertyDto)),
                }
            };
        }
    }
}
