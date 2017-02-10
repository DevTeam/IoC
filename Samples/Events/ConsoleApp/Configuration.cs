namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Contracts;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    internal class Configuration: IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Lifetimes);
            foreach (var config in container.Configure().DependsOn<JsonConfiguration>(ReadConfiguration("ClassLibrary.configuration.json")))
            {
                yield return config;
            }
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Register().Lifetime(Wellknown.Lifetime.Singleton).Contract<IConsole>().Autowiring<Console>();
        }

        private string ReadConfiguration([NotNull] string jsonFileName)
        {
            if (jsonFileName == null) throw new ArgumentNullException(nameof(jsonFileName));
            if (string.IsNullOrWhiteSpace(jsonFileName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(jsonFileName));
            var fullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
            return File.ReadAllText(fullName);
        }
    }
}
