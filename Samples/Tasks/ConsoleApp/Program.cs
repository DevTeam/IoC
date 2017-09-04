namespace ConsoleApp
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Autowiring<Program, Program>().ToSelf())
            {
                container.Resolve().Instance<Program>();
            }
        }

        public Program()
        {
            var cnt = 1000u;
            Environment.ProcessorCount

            var step = 1d / cnt;
            double sum = new Pi().Calculate(0, cnt, step);
            var pi = sum * step;
        }

        private class Pi
        {
            public double Calculate(ulong rangeStart, ulong rangeFinish, double step)
            {
                double sum = 0;
                for (var i = rangeStart; i < rangeFinish; i++)
                {
                    var x = (i + .5d) * step;
                    sum = sum + 4d / (1d + x * x);
                }

                return sum;
            }
        }
    }
}