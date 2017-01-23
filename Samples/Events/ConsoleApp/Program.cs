namespace ConsoleApp
{
    using Contracts;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main(string[] args)
        {
            using (var container = new Container())
            using (container.Configure().DependsOn<Configuration>().Apply())
            {
                container.Resolve().Instance<IEventBroker>();
                System.Console.ReadLine();
            }
        }
    }
}
