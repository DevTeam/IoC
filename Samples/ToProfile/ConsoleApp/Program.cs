namespace ConsoleApp
{
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;
    using DevTeam.IoC.Tests.Integration;

    public class Program
    {
        public static void Main()
        {
            using (var rootResolver = new Container("root").Configure()
                .DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                PerformanceTests.PerformanceTest(rootResolver, 100000);
            }
        }
    }
}
