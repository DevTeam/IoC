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
            using (var container = new Container()
                .Configure()
                .DependsOn(
                    Assembly.GetEntryAssembly(),
                    Assembly.Load(new AssemblyName("ClassLibrary"))
                ).Include())
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