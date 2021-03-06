﻿namespace DevTeam.IoC.Tests.Integration
{
    using System;
    using System.IO;
    using Configurations.Json;
    using Contracts;
    using Models;
    using Shouldly;
    using Xunit;

    public class PerformanceTests
    {

#if DEBUG
        private const int RepeatCount = 1000;
#else
        private const int RepeatCount = 100000;
#endif
        [Fact]
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

            trace.Output.Count.ShouldBeInRange(20, 24);
        }

        [Fact]
        public void SimplePerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < RepeatCount; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Fact]
        public void SimpleFactoryMethodPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Contract<ISimpleService>().FactoryMethod(ctx => new SimpleService()).ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < RepeatCount; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Fact]
        public void SimpleSingletonPerformanceTest()
        {
            using (var rootContainer = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Lifetime(Wellknown.Lifetime.Singleton).Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                var resolving = rootContainer.Resolve().Contract<ISimpleService>();
                for (var i = 0; i < RepeatCount; i++)
                {
                    resolving.Instance<ISimpleService>();
                }
            }
        }

        [Fact]
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

                for (var i = 0; i < RepeatCount; i++)
                {
                    container.Resolve().Instance<ISimpleService>();
                }
            }
        }

        [Fact]
        public void TestWhenJsonConfiguration()
        {
            var eventsConfigurationFile = Path.Combine(TestsExtensions.GetBinDirectory(), "EventsConfiguration.json");
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

            trace.Output.Count.ShouldBeInRange(20, 24);
        }

        [Fact]
        public void ResolvePerformanceTest()
        {
            using (var rootResolver = new Container("root")
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                PerformanceTest(rootResolver, 1000);
            }
        }

        [Fact]
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

        public static void PerformanceTest(IContainer rootResolver, int ticks)
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
}
