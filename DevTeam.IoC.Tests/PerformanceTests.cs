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
            using (var rootContainer = new Container("root"))
            using (rootContainer
                .Configure()
                .DependsOn(new EventsConfiguration(true))
                .Apply())
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
            using (var rootContainer = new Container("root"))
            using (rootContainer.Configure().DependsOn(Wellknown.Features.Default).Apply())
            using (rootContainer.Register().Contract<ISimpleService>().AsAutowiring<SimpleService>())
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
            using (var container = new Container("root"))
            {
                container.Register().Contract<IReferenceDescriptionResolver>().AsFactoryMethod<IReferenceDescriptionResolver>(ctx => new ReferenceDescriptionResolver());

                using (container
                    .Configure()
                    .DependsOn(Wellknown.Features.Default)
                    .DependsOn<JsonConfiguration>(json)
                    .Apply())
                {
                    var eventRegistry = container.Resolve().Instance<IEventRegistry>();
                    eventRegistry.RegisterEvent<DateTimeOffset>();

                    var timerManager = container.Resolve().Instance<ITimerManager>();
                    timerManager.Tick();

                    trace = container.Resolve().Instance<ITrace>();
                }
            }

            trace.Output.Count.ShouldBe(24);
        }

        [Test]
        [Repeat(5)]
        public void ResolvePerformanceTest()
        {
            using (var rootResolver = new Container("root"))
            using (rootResolver
                .Configure()
                .DependsOn(Wellknown.Features.Default)
                .Apply())
            {
                PerformanceTest(rootResolver, 1000);
            }
        }

        [Test]
        public void ConfigurePerformanceTest()
        {
            using (var rootResolver = new Container("root"))
            using (rootResolver
                .Configure()
                .DependsOn(Wellknown.Features.Default)
                .Apply())
            {
                for (var i = 0; i < 100; i++)
                {
                    PerformanceTest(rootResolver, 10);
                }
            }
        }

        private static void PerformanceTest(IResolver rootResolver, int ticks)
        {
            using (var childContainer = rootResolver.CreateChild())
            using (childContainer
                .Configure()
                .DependsOn(new EventsConfiguration(false))
                .Apply())
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

        private class SimpleService : ISimpleService
        {
        }
    }
}
