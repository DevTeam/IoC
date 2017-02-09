namespace ConsoleApp
{
    using System;
    using System.IO;
    using System.Reflection;
    using ClassLibrary;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            var jsonConfigStr = ReadConfiguration<IHelloWorld>("ClassLibrary.configuration.json");

            // Create the root container
            using (var container = new Container())
            // Appply the configuration from the json string
            using (container.Configure().DependsOn<JsonConfiguration>(jsonConfigStr).Apply())
            {
                // Resolve an instance implementing the interface "IHelloWorld"
                var helloWorld = container.Resolve().Instance<IHelloWorld>();
                
                // Run method to say "Hello"
                helloWorld.SayHello();
            }
        }

        private static string ReadConfiguration<TType>(string resourceName)
        {
            if (resourceName == null) throw new ArgumentNullException(nameof(resourceName));
            using (var configReader = new StreamReader(typeof(TType).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName)))
            {
                return configReader.ReadToEnd();
            }
        }
    }
}