namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;

    internal sealed class DtoFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new DtoFeature();

        private DtoFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LifetimesFeature.Shared;
            yield return ChildContainersFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<ITypeResolver, TypeResolver>();
            yield return container.Register().State<IConfigurationDto>(0).Autowiring<IConfiguration, ConfigurationDtoAdapter>();
            yield return container.Register().State<string>(0).Contract<IConfigurationDescriptionDto>().FactoryMethod(ctx => new ConfigurationDescriptionDto(ctx.GetState<string>(0)));
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<string, object, Type>, ConverterStringToObject>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<IValueDto, object, TypeResolverContext>, ConverterValueDtoToObject>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<ITagDto, object, TypeResolverContext>, ConverterTagDtoToObject>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<IParameterDto, IParameterMetadata, ConverterParameterDtoToParameterMetadata.Context>, ConverterParameterDtoToParameterMetadata>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<IRegisterDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterRegisterDtoToRegistations.Context>, ConverterRegisterDtoToRegistations>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<IConfigurationDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterConfigurationDtoToRegistrations.Context>, ConverterConfigurationDtoToRegistrations>();
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<IConverter<IConfigurationDto, IEnumerable<IConfiguration>, IContainer>, ConverterConfigurationDtoToDependencies>();
        }
    }
}
