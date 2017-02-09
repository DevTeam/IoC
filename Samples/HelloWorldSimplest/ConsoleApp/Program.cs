namespace ConsoleApp
{
    using System.Reflection;
    using ClassLibrary;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    [Contract(typeof(Program))]
    public class Program
    {
        public static void Main()
        {
            using (var container = new Container())
            using (container.Configure().DependsOn(
                Assembly.GetEntryAssembly(),
                Assembly.Load(new AssemblyName("ClassLibrary"))).Apply())
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