namespace ConsoleApp
{
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (var container = new Container().Configure()
                    .ToSelf())
            {
                // container.Resolve().Instance<Program>();
            }
        }
    }
}