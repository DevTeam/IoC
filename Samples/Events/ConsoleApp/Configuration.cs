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
        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver.Feature(Wellknown.Features.Lifetimes);
            foreach (var config in resolver.Configure().DependsOn<JsonConfiguration>(ReadConfiguration("ClassLibrary.configuration.json")))
            {
                yield return config;
            }
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver.Register().Lifetime(Wellknown.Lifetimes.Singleton).Contract<IConsole>().AsAutowiring<Console>();
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
