namespace ConsoleApp
{
    using System.IO;
    using ClassLibrary;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.CSharp;
    using DevTeam.IoC.Contracts;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Configure().DependsOn<CSharpConfiguration>(File.ReadAllText("Config.cs.txt")).ToSelf()
                .Register().Autowiring<Program, Program>().ToSelf())
            {
                container.Resolve().Instance<IHelloWorld>().SayHello();
            }
        }
    }
}
