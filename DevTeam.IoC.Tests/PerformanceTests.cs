namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Configurations.Json;
    using Contracts;
    using Microsoft.Practices.Unity;
    using Models;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    public class PerformanceTests
    {
        [Test]
        public void SimpleTest()
        {
            ITrace trace;
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(new EventsConfiguration(true)).ToSelf())
            {
                var eventRegistry = rootContainer.Resolve().Instance<IEventRegistry>();
                eventRegistry.RegisterEvent<DateTimeOffset>();

                var timerManager = rootContainer.Resolve().Instance<ITimerManager>();
                timerManager.Tick();

                trace = rootContainer.Resolve().Instance<ITrace>();
            }

            trace.Output.Count.ShouldBe(24);
        }

        [Test]
        public void SimplePerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < 100000; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void SimpleFactoryMethodPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().FactoryMethod(ctx => new SimpleService()).ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < 1000000; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void SimpleSingletonPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Lifetime(Wellknown.Lifetime.Singleton).Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < 100000; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void SimpleHierarchyPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                IContainer container = rootContainer;
                for (var i = 0; i < 1000; i++)
                {
                    container = container.CreateChild(i);
                }

                for (var i = 0; i < 10000; i++)
                {
                    container.Resolve().Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void TestWhenJsonConfiguration()
        {
            var eventsConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventsConfiguration.json");
            var json = File.ReadAllText(eventsConfigurationFile);
            ITrace trace;
            using (var container = new Container("root")
                .Register().Contract<IReferenceDescriptionResolver>().FactoryMethod<IReferenceDescriptionResolver>(ctx => new ReferenceDescriptionResolver()).ToSelf()
                .Configure()
                    .DependsOn(Wellknown.Feature.Default)
                    .DependsOn<JsonConfiguration>(json)
                .ToSelf())
            {
                var eventRegistry = container.Resolve().Instance<IEventRegistry>();
                eventRegistry.RegisterEvent<DateTimeOffset>();

                var timerManager = container.Resolve().Instance<ITimerManager>();
                timerManager.Tick();

                trace = container.Resolve().Instance<ITrace>();
            }

            trace.Output.Count.ShouldBe(24);
        }

        [Test]
        [Repeat(5)]
        public void ResolvePerformanceTest()
        {
            using (var rootResolver = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                PerformanceTest(rootResolver, 1000);
            }
        }

        [Test]
        public void ConfigurePerformanceTest()
        {
            using (var rootResolver = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                for (var i = 0; i < 100; i++)
                {
                    PerformanceTest(rootResolver, 10);
                }
            }
        }

        private static readonly Dictionary<string, Func<int, long>> Iocs = new Dictionary<string, Func<int, long>>()
        {
            {"Unity", Unity},
            {"DevTeam", DevTeam}
        };

        [Test]
        public void ComparisonTest()
        {
            const int warmupSeries = 10;
            const int series = 100000;

            foreach (var ioc in Iocs)
            {
                ioc.Value(warmupSeries);
            }

            var results = new Dictionary<string, long>();
            foreach (var ioc in Iocs)
            {
                var elapsedMilliseconds = ioc.Value(series);
                Debug.WriteLine($"{ioc.Key}: {elapsedMilliseconds}");
                results.Add(ioc.Key, elapsedMilliseconds);
            }

            var actualElapsedMilliseconds = results["DevTeam"];
            foreach (var result in results)
            {
                Assert.LessOrEqual(actualElapsedMilliseconds, result.Value, $"{result.Key} is better: {result.Value}, our result is : {actualElapsedMilliseconds}");
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

        private static void PerformanceTest(IContainer rootResolver, int ticks)
        {
            using (var childContainer = rootResolver.CreateChild("child")
                .Configure().DependsOn(new EventsConfiguration(false)).ToSelf())
            {
                var eventRegistry = childContainer.Resolve().Instance<IEventRegistry>();
                eventRegistry.RegisterEvent<DateTimeOffset>();

                var timerManager = childContainer.Resolve().Instance<ITimerManager>();
                for (var i = 0; i < ticks; i++)
                {
                    timerManager.Tick();
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SimpleService : ISimpleService
        {
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
