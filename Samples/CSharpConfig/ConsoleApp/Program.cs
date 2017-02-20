namespace ConsoleApp
{
    using System.IO;
    using DevTeam.IoC;
    using DevTeam.IoC.Configurations.CSharp;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main(string[] args)
        {
            using (var container = new Container()
                .Configure().DependsOn<CSharpConfiguration>(File.ReadAllText("Config.cs.txt")).ToSelf()
                .Register().Contract<Program>().Autowiring(typeof(Program)).ToSelf())
            {
                container.Resolve().Instance<Program>();
            }
        }
    }
}
