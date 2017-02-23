using System;
using System.Collections.Generic;
using System.Diagnostics;
using DevTeam.IoC;
using DevTeam.IoC.Contracts;
using Microsoft.Practices.Unity;

namespace ComparisonApp
{
    public class Program
    {
        private static readonly Dictionary<string, Action<int>> Iocs = new Dictionary<string, Action<int>>()
        {
            {"Unity", Unity},
            {"DevTeam", DevTeam}
        };

        public static void Main(string[] args)
        {
            const int warmupSeries = 10;
            const int series = 100000;

            using (new Case("Warmup"))
            {
                foreach (var ioc in Iocs)
                {
                    ioc.Value(warmupSeries);
                }
            }

            foreach (var ioc in Iocs)
            {
                using (new Case(ioc.Key))
                {
                    ioc.Value(series);
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public class Case: IDisposable
        {
            private readonly string _name;
            private readonly Stopwatch _stopwatch;

            public Case(string name)
            {
                _name = name;
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                Console.WriteLine($"{_name}: {_stopwatch.ElapsedMilliseconds}");
            }
        }

        public static void DevTeam(int series)
        {
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<IService1>().Autowiring<Service1>()
                .And().Contract<IService2>().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<Service2>().ToSelf())
            {
                for (var i = 0; i < series; i++)
                {
                    container.Resolve().Instance<IService1>();
                }
            }
        }

        public static void Unity(int series)
        {
            using (var container = new UnityContainer())
            {
                container.RegisterType<IService1, Service1>();
                container.RegisterType<IService2, Service2>(new ContainerControlledLifetimeManager());
                for (var i = 0; i < series; i++)
                {
                    container.Resolve(typeof(IService1));
                }
            }
        }
    }

    public interface IService1
    {
    }

    public class Service1 : IService1
    {
        public Service1(IService2 service2)
        {
            if (service2 == null) throw new ArgumentNullException(nameof(service2));
        }
    }

    public interface IService2
    {
    }

    public class Service2 : IService2
    {
    }
}
