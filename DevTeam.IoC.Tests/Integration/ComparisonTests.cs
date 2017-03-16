namespace DevTeam.IoC.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Contracts;
    using Microsoft.Practices.Unity;
    using Ninject;
    using NUnit.Framework;

    [TestFixture]
    [Category("Long")]
    public class ComparisonTests
    {
        private static readonly Dictionary<string, Func<int, long>> Iocs = new Dictionary<string, Func<int, long>>()
        {
            {"Unity", Unity},
            {"DevTeam", DevTeam},
            {"Ninject", Ninject},
        };

        [Test]
        public void ComparisonTest()
        {
#if DEBUG
            Assert.Inconclusive("Inconclusive for DEBUG build");
#endif
            const int warmupSeries = 10;
            const int series = 100000;
            const int pressure = 1 << 32;

            foreach (var ioc in Iocs)
            {
                ioc.Value(warmupSeries);
            }

            var results = new Dictionary<string, long>();
            foreach (var ioc in Iocs)
            {
                GC.AddMemoryPressure(pressure);
                GC.Collect();
                GC.RemoveMemoryPressure(pressure);

                var elapsedMilliseconds = ioc.Value(series);
                Trace.WriteLine($"{ioc.Key}: {elapsedMilliseconds}");
                results.Add(ioc.Key, elapsedMilliseconds);
            }

            var actualElapsedMilliseconds = results["DevTeam"];
            var resultsStr = string.Join("\n", results.Select(i => $"{i.Key}: {i.Value}"));
            var resultFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ComparisonTest.txt");
            File.WriteAllText(resultFileName, resultsStr);
            foreach (var result in results)
            {
                Assert.LessOrEqual(actualElapsedMilliseconds, result.Value, $"{result.Key} is better: {result.Value}, our result is : {actualElapsedMilliseconds}.\nResults:\n{resultsStr}");
            }
        }

        public static long DevTeam(int series)
        {
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<IService1>().Autowiring<Service1>()
                .And().Contract<IService2>().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<Service2>().ToSelf())
            {
                var resolver = container.Resolve().Contract<IService1>();
                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < series; i++)
                {
                    resolver.Instance<IService1>();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
        }

        public static long Unity(int series)
        {
            using (var container = new UnityContainer())
            {
                container.RegisterType<IService1, Service1>();
                container.RegisterType<IService2, Service2>(new ContainerControlledLifetimeManager());
                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < series; i++)
                {
                    container.Resolve(typeof(IService1));
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
        }

        public static long Ninject(int series)
        {
            using (var kernel = new StandardKernel())
            {
                kernel.Bind<IService1>().To<Service1>();
                kernel.Bind<IService2>().To<Service2>().InSingletonScope();
                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < series; i++)
                {
                    kernel.Get<IService1>();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
        }
    }

    public interface IService1
    {
    }

    public class Service1 : IService1
    {
        public Service1([NotNull] IService2 service2)
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
