namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConfigurationFromSringData: IConfiguration
    {
        private readonly IResolver _resolver;
        private readonly Type _configurationType;
        private readonly Lazy<IConfiguration> _configuration;

        public ConfigurationFromSringData(
            [NotNull] IResolver resolver,
            [NotNull] Type configurationType,
            [NotNull] string description)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (description.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            _resolver = resolver;
            _configurationType = configurationType;
            _configuration = new Lazy<IConfiguration>(() => CreateConfiguration(description));
        }

        internal IConfiguration BaseConfiguration => _configuration.Value;

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            foreach (var dependency in _configuration.Value.GetDependencies(container))
            {
                yield return dependency;
            }
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return _configuration.Value.Apply(container);
        }

        private IConfiguration CreateConfiguration([NotNull] string description)
        {
            if (description.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            var configurationDescriptionDto = _resolver.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(description);
            IConfiguration configuration;
            if(_resolver.Resolve().Tag(_configurationType).State<IConfigurationDescriptionDto>(0).TryInstance(out configuration, configurationDescriptionDto))
            {
                return configuration;
            }

            var configurationDto = _resolver.Resolve().Tag(_configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
            return _resolver.Resolve().State<IConfigurationDto>(0).Instance<IConfiguration>(configurationDto);
        }
    }
}