namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class Configuring<T> : IConfiguring<T>
        where T : IContainer
    {
        private readonly T _resolver;
        private readonly List<HashSet<IConfiguration>> _configurations = new List<HashSet<IConfiguration>>();
        private readonly HashSet<IConfiguration> _appliedConfigurations = new HashSet<IConfiguration>();
        private readonly List<IDisposable> _registrations = new List<IDisposable>();

        public Configuring(T resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _resolver = resolver;
        }

        public IConfiguring<T> DependsOn(params IConfiguration[] configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            if (configurations.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(configurations));
            _configurations.Add(new HashSet<IConfiguration>(configurations));
            return this;
        }

        public IConfiguring<T> DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new()
        {
            return DependsOn(new TConfiguration());
        }

        public IConfiguring<T> DependsOn(params Wellknown.Feature[] features)
        {
            if (features == null) throw new ArgumentNullException(nameof(features));
            if (features.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(features));
            _configurations.Add(new HashSet<IConfiguration>(features.Distinct().Select(wellknownConfiguration => _resolver.Resolve<IResolver>().Tag(wellknownConfiguration).Instance<IConfiguration>())));
            return this;
        }

        public IConfiguring<T> DependsOn(Type configurationType, string description)
        {
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            _configurations.Add(new HashSet<IConfiguration> { DtoFeature.Shared, (IConfiguration)Activator.CreateInstance(configurationType) });
            _configurations.Add(new HashSet<IConfiguration> { _resolver.Resolve().State<Type>(0).State<string>(1).Instance<IConfiguration>(configurationType, description) });
            return this;
        }

        public IConfiguring<T> DependsOn<TConfiguration>(string description) where TConfiguration : IConfiguration, new()
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            return DependsOn(typeof(TConfiguration), description);
        }

        public IConfiguring<T> DependsOn([NotNull] params Assembly[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            if (assemblies.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(assemblies));
            DependsOn(assemblies.Distinct().Select(CreateConfigurationFromAssembly).ToArray());
            return this;
        }

        private IDisposable Apply()
        {
            _registrations.AddRange(_configurations.Select(Apply));
            var registration = new CompositeDisposable(_registrations);
            _registrations.Clear();
            return registration;
        }

        public T ToSelf()
        {
            _resolver.Resolve().Instance<IInternalResourceStore>().AddResource(Apply());
            return _resolver;
        }

        public IConfiguration Create()
        {
            var configuration = new CompositeConfiguration(_configurations.SelectMany(i => i).Distinct().ToList());
            _configurations.Clear();
            return configuration;
        }

        private IDisposable Apply([NotNull] IEnumerable<IConfiguration> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            return new CompositeDisposable(ApplyConfigurations(configuration));
        }

        private IEnumerable<IDisposable> ApplyConfigurations(IEnumerable<IConfiguration> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            using (var enumerator = (configurations as IConfiguration[] ?? configurations).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!_appliedConfigurations.Add(enumerator.Current))
                    {
                        continue;
                    }

                    foreach (var disposable in ApplyConfigurations(enumerator.Current.GetDependencies(_resolver)))
                    {
                        yield return disposable;
                    }

                    foreach (var disposable in enumerator.Current.Apply(_resolver))
                    {
                        yield return disposable;
                    }
                }
            }
        }

        private IConfiguration CreateConfigurationFromAssembly(Assembly assembly)
        {
            return _resolver.Resolve().State<Assembly>(0).Instance<IConfiguration>(assembly);
        }
    }
}