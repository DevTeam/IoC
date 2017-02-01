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
        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return resolver.Configuration(Wellknown.Features.Lifetimes);
            foreach (var config in resolver.Configure().DependsOn<JsonConfiguration>(ReadConfiguration("ClassLibrary.configuration.json")))
            {
                yield return config;
            }
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield return resolver.Register().Lifetime(Wellknown.Lifetimes.Singleton).Contract<IConsole>().AsAutowiring<Console>();
        }

        private string ReadConfiguration(string jsonFileName)
        {
            if (jsonFileName == null) throw new ArgumentNullException(nameof(jsonFileName));
            var fulllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
            return File.ReadAllText(fulllName);
        }
    }
}
