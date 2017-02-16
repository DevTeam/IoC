namespace ConsoleApp
{
    using System.IO;
    using Contracts;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Configure().Dependency<JsonConfiguration>(File.ReadAllText("configuration.json")).ToSelf())
            {
                container.Resolve().Instance<IEventBroker>();
                System.Console.ReadLine();
            }

            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadLine();
        }
    }
}
