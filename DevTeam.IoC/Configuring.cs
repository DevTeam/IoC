namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class Configuring<TContainer> : IConfiguring<TContainer>
        where TContainer : IContainer
    {
        private readonly TContainer _container;
        private readonly List<HashSet<IConfiguration>> _configurations = new List<HashSet<IConfiguration>>();
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private readonly HashSet<IConfiguration> _appliedConfiguration = new HashSet<IConfiguration>();

        public Configuring(TContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
        }

        public IConfiguring<TContainer> DependsOn(params IConfiguration[] configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            if (configurations.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(configurations));
            _configurations.Add(new HashSet<IConfiguration>(configurations));
            return this;
        }

        public IConfiguring<TContainer> DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new()
        {
            return DependsOn(new TConfiguration());
        }

        public IConfiguring<TContainer> DependsOn(params Wellknown.Feature[] features)
        {
            if (features == null) throw new ArgumentNullException(nameof(features));
            if (features.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(features));
            _configurations.Add(new HashSet<IConfiguration>(features.Distinct().Select(wellknownConfiguration => _container.Resolve<IResolver>().Tag(wellknownConfiguration).Instance<IConfiguration>())));
            return this;
        }

        public IConfiguring<TContainer> DependsOn(Type configurationType, string description)
        {
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (description.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            _configurations.Add(new HashSet<IConfiguration> { DtoFeature.Shared, (IConfiguration)Activator.CreateInstance(configurationType) });
            _configurations.Add(new HashSet<IConfiguration> { _container.Resolve().State<Type>(0).State<string>(1).Instance<IConfiguration>(configurationType, description) });
            return this;
        }

        public IConfiguring<TContainer> DependsOn<TConfiguration>(string description) where TConfiguration : IConfiguration, new()
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (description.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            return DependsOn(typeof(TConfiguration), description);
        }

        public IConfiguring<TContainer> DependsOn([NotNull] params Assembly[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            if (assemblies.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(assemblies));
            DependsOn(assemblies.Distinct().Select(CreateConfigurationFromAssembly).ToArray());
            return this;
        }

        public IDisposable Apply()
        {
            _registrations.AddRange(_configurations.Select(Apply));
            var registration = new CompositeDisposable(_registrations);
            _registrations.Clear();
            return registration;
        }

        public TContainer ToSelf()
        {
            _container.Resolve().Instance<IInternalResourceStore>().AddResource(Apply());
            return _container;
        }

        public IConfiguration Create()
        {
            return new CompositeConfiguration(GetConfigurations(_configurations.SelectMany(i => i)).ToList());
        }

        private IDisposable Apply([NotNull] IEnumerable<IConfiguration> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            return new CompositeDisposable(ApplyConfigurations(configuration));
        }

        private IEnumerable<IConfiguration> GetConfigurations(
            IEnumerable<IConfiguration> configurations,
            HashSet<IConfiguration> allConfigurations = null)
        {
            allConfigurations = allConfigurations ?? new HashSet<IConfiguration>();
            using (var enumerator = (configurations as IConfiguration[] ?? configurations).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!allConfigurations.Add(enumerator.Current))
                    {
                        continue;
                    }

                    var curConfig = enumerator.Current ?? throw new InvalidOperationException("Invalid state of configuration");
                    foreach (var config in GetConfigurations(curConfig.GetDependencies(_container), allConfigurations))
                    {
                        yield return config;
                    }

                    yield return enumerator.Current;
                }
            }
        }

        private IEnumerable<IDisposable> ApplyConfigurations(
            IEnumerable<IConfiguration> configurations)
        {
            return 
                from config in GetConfigurations(configurations)
                where _appliedConfiguration.Add(config)
                from registration in config.Apply(_container)
                select registration;
        }

        private IConfiguration CreateConfigurationFromAssembly(Assembly assembly)
        {
            return _container.Resolve().State<Assembly>(0).Instance<IConfiguration>(assembly);
        }
    }
}