namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
#if NET35
    using System.Linq;
#endif
    using System.Reflection;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConfigurationDtoAdapter : IConfiguration
    {
        [NotNull] private readonly IConfigurationDto _configurationDto;
        [NotNull] private readonly IConverter<IConfigurationDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterConfigurationDtoToRegistrations.Context> _converterConfigurationDtoToRegistrations;
        [NotNull] private readonly IConverter<IConfigurationDto, IEnumerable<IConfiguration>, IContainer> _converterConfigurationDtoToDependencies;

        internal IConfigurationDto ConfigurationDto => _configurationDto;

        public ConfigurationDtoAdapter(
            [NotNull] [State] IConfigurationDto configurationDto,
            [NotNull] IConverter<IConfigurationDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterConfigurationDtoToRegistrations.Context> converterConfigurationDtoToRegistrations,
            [NotNull] IConverter<IConfigurationDto, IEnumerable<IConfiguration>, IContainer> converterConfigurationDtoToDependencies)
        {
            _configurationDto = configurationDto ?? throw new ArgumentNullException(nameof(configurationDto));
            _converterConfigurationDtoToRegistrations = converterConfigurationDtoToRegistrations ?? throw new ArgumentNullException(nameof(converterConfigurationDtoToRegistrations));
            _converterConfigurationDtoToDependencies = converterConfigurationDtoToDependencies ?? throw new ArgumentNullException(nameof(converterConfigurationDtoToDependencies));
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (!_converterConfigurationDtoToDependencies.TryConvert(
                _configurationDto,
                out var dependencies,
                container))
            {
                throw new ContainerException($"Error during getting dependencies.\nDetails:\n{container}");
            }

            return dependencies;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (!_converterConfigurationDtoToRegistrations.TryConvert(
                _configurationDto,
                out var registrations,
                new ConverterConfigurationDtoToRegistrations.Context(
                    container,
                    new TypeResolverContext(
                        new List<Assembly>(),
                        new List<string>()))))
            {
                throw new ContainerException($"Error during getting registrations.\nDetails:\n{container}");
            }

#if NET35
            return registrations.Cast<IDisposable>();
#else
            return registrations;
#endif
        }
    }
}