namespace ConsoleApp
{
    using System.IO;
    using System.Threading;
    using Contracts;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.Json;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        // TODO: use async main with C# 7.1
        public static void Main()
        {
            using (var container = new Container()
                .Configure().DependsOn<JsonConfiguration>(File.ReadAllText("configuration.json")).ToSelf())
            {
                var eventBrokerCreation = container.Resolve().AsyncInstance<IEventBroker>(CancellationToken.None);
                eventBrokerCreation.Start();
                eventBrokerCreation.Wait();
                System.Console.ReadLine();
            }

            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadLine();
        }
    }
}
