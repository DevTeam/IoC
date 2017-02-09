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
                .DependsOn(new EventsConfiguration(true))
                .Include())
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
                .Configure()
                .DependsOn(Wellknown.Feature.Default)
                .Register(r => r.Contract<ISimpleService>().AsAutowiring<SimpleService>())
                .Include())
            {
                for (var i = 0; i < 10000; i++)
                {
                    rootContainer.Resolve().Instance<ISimpleService>();
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
                .Configure()
                .Register(r => r.Contract<IReferenceDescriptionResolver>().AsFactoryMethod<IReferenceDescriptionResolver>(ctx => new ReferenceDescriptionResolver()))
                .DependsOn(Wellknown.Feature.Default)
                .DependsOn<JsonConfiguration>(json)
                .Include())
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
                .DependsOn(Wellknown.Feature.Default)
                .Include())
            {
                PerformanceTest(rootResolver, 1000);
            }
        }

        [Test]
        public void ConfigurePerformanceTest()
        {
            using (var rootResolver = new Container("root")
                .Configure()
                .DependsOn(Wellknown.Feature.Default)
                .Include())
            {
                for (var i = 0; i < 100; i++)
                {
                    PerformanceTest(rootResolver, 10);
                }
            }
        }

        private static void PerformanceTest(IResolver rootResolver, int ticks)
        {
            using (var childContainer = rootResolver.CreateChild("child")
                .Configure()
                .DependsOn(new EventsConfiguration(false))
                .Include())
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
