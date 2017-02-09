namespace ConsoleApp
{
    using Contracts;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Configure()
                .DependsOn<Configuration>()
                .Include())
            {
                container.Resolve().Instance<IEventBroker>();
                System.Console.ReadLine();
            }

            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadLine();
        }
    }
}
