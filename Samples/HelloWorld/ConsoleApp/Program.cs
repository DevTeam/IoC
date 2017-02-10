namespace ConsoleApp
{
    using System.IO;
    using System.Reflection;
    using ClassLibrary;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    [Contract(typeof(Program))]
    public class Program
    {
        public static void Main()
        {
            var jsonConfigStr = ReadConfiguration<IHelloWorld>("ClassLibrary.configuration.json");

            using (
                var container = new Container()
                .Configure()
                .DependsOn<JsonConfiguration>(jsonConfigStr)
                .Register().AsAutowiring(typeof(Program))
                .Own())
            {
                container.Resolve().Instance<Program>();
            }
        }

        public Program(IHelloWorld helloWorld)
        {
            helloWorld.SayHello();
        }

        private static string ReadConfiguration<TType>(string resourceName)
        {
            using (var configReader = new StreamReader(typeof(TType).GetTypeInfo().Assembly.GetManifestResourceStream(resourceName)))
            {
                return configReader.ReadToEnd();
            }
        }
    }
}