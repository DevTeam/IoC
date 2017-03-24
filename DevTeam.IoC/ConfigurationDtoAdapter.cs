namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            if (configurationDto == null) throw new ArgumentNullException(nameof(configurationDto));
            if (converterConfigurationDtoToRegistrations == null) throw new ArgumentNullException(nameof(converterConfigurationDtoToRegistrations));
            if (converterConfigurationDtoToDependencies == null) throw new ArgumentNullException(nameof(converterConfigurationDtoToDependencies));
            _configurationDto = configurationDto;
            _converterConfigurationDtoToRegistrations = converterConfigurationDtoToRegistrations;
            _converterConfigurationDtoToDependencies = converterConfigurationDtoToDependencies;
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            IEnumerable<IConfiguration> dependencies;
            if (!_converterConfigurationDtoToDependencies.TryConvert(_configurationDto, out dependencies, container))
            {
                throw new InvalidOperationException("Error during getting configurations");
            }

            return dependencies;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (!_converterConfigurationDtoToRegistrations.TryConvert(_configurationDto, out IEnumerable<IRegistrationResult<IContainer>> registrations, new ConverterConfigurationDtoToRegistrations.Context(container, new TypeResolverContext(new List<Assembly>(), new List<string>()))))
            {
                throw new InvalidOperationException("Error during applieng a configuration");
            }

            return registrations.Cast<IDisposable>();
        }
    }
}