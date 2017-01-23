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
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield return resolver.Register().Contract<IConsole>().AsAutowiring<Console>();
            yield return resolver.Configure().DependsOn<JsonConfiguration>(ReadConfiguration("ClassLibrary.configuration.json")).Apply();
        }

        private string ReadConfiguration(string jsonFileName)
        {
            if (jsonFileName == null) throw new ArgumentNullException(nameof(jsonFileName));
            var fulllName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
            return File.ReadAllText(fulllName);
        }
    }
}
