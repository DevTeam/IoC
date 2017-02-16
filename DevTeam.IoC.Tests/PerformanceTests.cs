namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Configurations.Json;
    using Contracts;
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
                .Configure()
                .Dependency(new EventsConfiguration(true))
                .ToSelf())
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
                .Configure().Dependency(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                for (var i = 0; i < 100000; i++)
                {
                    rootContainer.Resolve().Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void SimpleSingletonPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().Dependency(Wellknown.Feature.Default).ToSelf()
                .Register().Lifetime(Wellknown.Lifetime.Singleton).Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                for (var i = 0; i < 100000; i++)
                {
                    rootContainer.Resolve().Instance<ISimpleService>();
                }
            }
        }

        [Test]
        public void SimpleHierarchyPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().Dependency(Wellknown.Feature.Default).ToSelf()
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
                .Register().Contract<IReferenceDescriptionResolver>().FactoryMethod<IReferenceDescriptionResolver>(ctx => new ReferenceDescriptionResolver())
                .ToSelf()
                .Configure()
                .Dependency(Wellknown.Feature.Default)
                .Dependency<JsonConfiguration>(json)
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
                .Configure()
                .Dependency(Wellknown.Feature.Default)
                .ToSelf())
            {
                PerformanceTest(rootResolver, 1000);
            }
        }

        [Test]
        public void ConfigurePerformanceTest()
        {
            using (var rootResolver = new Container("root")
                .Configure()
                .Dependency(Wellknown.Feature.Default)
                .ToSelf())
            {
                for (var i = 0; i < 100; i++)
                {
                    PerformanceTest(rootResolver, 10);
                }
            }
        }

        private static void PerformanceTest(IContainer rootResolver, int ticks)
        {
            using (var childContainer = rootResolver.CreateChild("child")
                .Configure()
                .Dependency(new EventsConfiguration(false))
                .ToSelf())
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
}
