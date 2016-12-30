namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Dto;

    internal class Configuring : IConfiguring
    {
        private readonly IResolver _resolver;
        private readonly List<HashSet<IConfiguration>> _configurations = new List<HashSet<IConfiguration>>();
        private readonly HashSet<IConfiguration> _appliedConfigurations = new HashSet<IConfiguration>();

        public Configuring(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _resolver = resolver;
        }

        public IConfiguring DependsOn(params IConfiguration[] configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            _configurations.Add(new HashSet<IConfiguration>(configurations));
            return this;
        }

        public IConfiguring DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new()
        {
            return DependsOn(new TConfiguration());
        }

        public IConfiguring DependsOn(params Wellknown.Configurations[] configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            _configurations.Add(new HashSet<IConfiguration>(configurations.Select(wellknownConfiguration => _resolver.Resolve().Tag(wellknownConfiguration).Instance<IConfiguration>())));
            return this;
        }

        public IConfiguring DependsOn(Type configurationType, string description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            _configurations.Add(new HashSet<IConfiguration> { DtoConfiguration.Shared, (IConfiguration)Activator.CreateInstance(configurationType) });
            _configurations.Add(new HashSet<IConfiguration> { new ConfigurationFromDto(_resolver, configurationType, description) });
            return this;
        }

        public IConfiguring DependsOn<TConfiguration>(string description) where TConfiguration : IConfiguration, new()
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            return DependsOn(typeof(TConfiguration), description);
        }

        public IDisposable Apply()
        {
            return new CompositeDisposable(_configurations.Select(Apply));
        }

        private IDisposable Apply(IEnumerable<IConfiguration> configuration)
        {
            var configs = ExpandConfigurations(_resolver, configuration).Distinct().ToArray();
            return new CompositeDisposable(configs.Where(config => _appliedConfigurations.Add(config)).Select(config => config.Apply(_resolver)).SelectMany(disposable => disposable));
        }

        private IEnumerable<IConfiguration> ExpandConfigurations(IResolver resolver, IEnumerable<IConfiguration> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            var enumerable = configurations as IConfiguration[] ?? configurations.ToArray();
            return enumerable.Select(dependency => ExpandConfigurations(resolver, dependency.GetDependencies(resolver))).SelectMany(i => i).Concat(enumerable);
        }

        private class ConfigurationFromDto: IConfiguration
        {
            private readonly IResolver _resolver;
            private readonly Type _configurationType;
            private readonly Lazy<IConfiguration> _configuration;

            public ConfigurationFromDto(IResolver resolver, Type configurationType, string description)
            {
                _resolver = resolver;
                _configurationType = configurationType;
                _configuration = new Lazy<IConfiguration>(() => CreateConfiguration(description));
            }

            private IConfiguration CreateConfiguration(string description)
            {
                var configurationDescriptionDto = _resolver.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(description);
                var configurationDto = _resolver.Resolve().Tag(_configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
                return _resolver.Resolve().State<IConfigurationDto>(0).Instance<IConfiguration>(configurationDto);
            }

            public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
            {
                foreach (var dependency in _configuration.Value.GetDependencies(resolver))
                {
                    yield return dependency;
                }
            }

            public IEnumerable<IDisposable> Apply(IResolver resolver)
            {
                return _configuration.Value.Apply(resolver);
            }
        }
    }
}