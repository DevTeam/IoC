namespace ConsoleApp
{
    using System.IO;
    using ClassLibrary;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    [Contract(typeof(Program))]
    public class Program
    {
        public static void Main()
        {
            var iocJson = File.ReadAllText("IoC.json");

            // Create the root container and apply the configuration from the json string
            using (var container = new Container())
            using (container.Configure().DependsOn<JsonConfiguration>(iocJson).Apply())
            {
                container.Resolve().Instance<Program>();
            }
        }

        public Program(IHelloWorld helloWorld)
        {
            helloWorld.SayHello();
        }
    }
}