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
        public static void Main(string[] args)
        {
            using (var container = new Container())
            using (container.Configure().DependsOn<JsonConfiguration>(ReadConfiguration<IHelloWorld>("ClassLibrary.configuration.json")).Apply())
            {
                var helloWorld = container.Resolve().Instance<IHelloWorld>();
                helloWorld.SayHello();
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
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