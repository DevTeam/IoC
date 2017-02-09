namespace DevTeam.IoC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class Configuring<T> : IConfiguring<T>
        where T : IResolver, IDisposable
    {
        private readonly T _resolver;
        private readonly List<HashSet<IConfiguration>> _configurations = new List<HashSet<IConfiguration>>();
        private readonly HashSet<IConfiguration> _appliedConfigurations = new HashSet<IConfiguration>();

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

        public IConfiguring<T> DependsOn(params Wellknown.Features[] features)
        {
            if (features == null) throw new ArgumentNullException(nameof(features));
            if (features.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(features));
            _configurations.Add(new HashSet<IConfiguration>(features.Distinct().Select(wellknownConfiguration => _resolver.Resolve().Tag(wellknownConfiguration).Instance<IConfiguration>())));
            return this;
        }

        public IConfiguring<T> DependsOn(Type configurationType, string description)
        {
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (configurationType == null) throw new ArgumentNullException(nameof(configurationType));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
            _configurations.Add(new HashSet<IConfiguration> { DtoFeature.Shared, (IConfiguration)Activator.CreateInstance(configurationType) });
            _configurations.Add(new HashSet<IConfiguration> { new ConfigurationFromDto(_resolver, configurationType, description) });
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
            DependsOn(assemblies.Distinct().Select(assembly => (IConfiguration)new ConfigurationFromAssembly(assembly)).ToArray());
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IConfiguration> GetEnumerator()
        {
            return _configurations.SelectMany(i => i).GetEnumerator();
        }

        public IConfiguredResolver Apply()
        {
            var registration = new CompositeDisposable(_configurations.Select(Apply));
            return new ResolverWithRegistration<T>(_resolver, registration);
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

        private interface IResolverWithRegistration
        {
            IResolver BaseResolver { get; }
        }

        private class ResolverWithRegistration<TResolver> : IConfiguredResolver, IResolverWithRegistration
            where TResolver: IResolver, IDisposable
        {
            private readonly TResolver _baseResolver;
            private readonly IDisposable _registration;

            public ResolverWithRegistration([NotNull] TResolver baseResolver, [NotNull] IDisposable registration)
            {
                if (baseResolver == null) throw new ArgumentNullException(nameof(baseResolver));
                if (registration == null) throw new ArgumentNullException(nameof(registration));
                _baseResolver = baseResolver;
                _registration = registration;
            }

            public IResolver BaseResolver => _baseResolver;

            public IKeyFactory KeyFactory => _baseResolver.KeyFactory;

            public bool TryCreateContext(ICompositeKey key, out IResolverContext resolverContext, IStateProvider stateProvider = null)
            {
                return _baseResolver.TryCreateContext(key, out resolverContext, stateProvider);
            }

            public object Resolve(IResolverContext context)
            {
                return _baseResolver.Resolve(context);
            }

            public void Dispose()
            {
                _baseResolver.Dispose();
                _registration.Dispose();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                var resolverWithRegistration = obj as IResolverWithRegistration;
                if (resolverWithRegistration != null)
                {
                    return Equals(resolverWithRegistration.BaseResolver, _baseResolver);
                }

                return Equals(obj, _baseResolver);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<TResolver>.Default.GetHashCode(_baseResolver);
            }
        }
    }
}