﻿// ReSharper disable HeuristicUnreachableCode
// ReSharper disable RedundantUsingDirective
#pragma warning disable 162
namespace DevTeam.IoC.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
#if !NET35 && !NETCOREAPP1_0&& !NETCOREAPP2_0
    using Microsoft.Practices.Unity;
#endif
    using Ninject;
    using Xunit;

    public class ComparisonTests
    {
        private static readonly Dictionary<string, Func<int, long>> Iocs = new Dictionary<string, Func<int, long>>()
        {
#if !NET35 && !NETCOREAPP1_0&& !NETCOREAPP2_0
            {"Unity", Unity},
#endif
            {"DevTeam", DevTeam},
            {"Ninject", Ninject},
        };

        [Fact]
        public void ComparisonTest()
        {
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

            var resultsStr = string.Join("\n", results.Select(i => $"{i.Key}: {i.Value}").ToArray());
            var resultFileName = Path.Combine(TestsExtensions.GetBinDirectory(), "ComparisonTest.txt");
            File.WriteAllText(resultFileName, resultsStr);
#if !DEBUG
            var actualElapsedMilliseconds = results["DevTeam"];
            foreach (var result in results)
            {
                Assert.True(actualElapsedMilliseconds <= result.Value, $"{result.Key} is better: {result.Value}, our result is : {actualElapsedMilliseconds}.\nResults:\n{resultsStr}");
            }
#endif
        }

        private static long DevTeam(int series)
        {
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<IService1>().Autowiring<Service1>()
                .And().Contract<IService2>().Lifetime(Wellknown.Lifetime.Singleton).Autowiring<Service2>().ToSelf())
            {
                var stopwatch = Stopwatch.StartNew();
                var resolver = container.Resolve().Contract<IService1>();
                for (var i = 0; i < series; i++)
                {
                    resolver.Instance<IService1>();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
        }

#if !NET35 && !NETCOREAPP1_0 && !NETCOREAPP2_0
        private static long Unity(int series)
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
#endif

        private static long Ninject(int series)
        {
#pragma warning disable 618
            using (var kernel = new StandardKernel())
#pragma warning restore 618
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

    // ReSharper disable once ClassNeverInstantiated.Global
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

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Service2 : IService2
    {
    }
}
