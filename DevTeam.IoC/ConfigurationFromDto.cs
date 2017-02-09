namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;

    internal class ConfigurationFromDto: IConfiguration
    {
        private readonly IResolver _resolver;
        private readonly Type _configurationType;
        private readonly Lazy<IConfiguration> _configuration;

        public ConfigurationFromDto(
            [NotNull] IResolver resolver,
            [NotNull] Type configurationType,
            [NotNull] string description)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            _resolver = resolver;
            _configurationType = configurationType;
            _configuration = new Lazy<IConfiguration>(() => CreateConfiguration(description));
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            foreach (var dependency in _configuration.Value.GetDependencies(resolver))
            {
                yield return dependency;
            }
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return _configuration.Value.Apply(resolver);
        }

        private IConfiguration CreateConfiguration([NotNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            var configurationDescriptionDto = _resolver.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(description);
            var configurationDto = _resolver.Resolve().Tag(_configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
            return _resolver.Resolve().State<IConfigurationDto>(0).Instance<IConfiguration>(configurationDto);
        }
    }
}