namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    public class EventsConfiguration: IConfiguration
    {
        private readonly bool _trace;

        public EventsConfiguration(bool trace)
        {
            _trace = trace;
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Default);
            yield return new PlatformConfiguration(_trace);
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var childContainer = container.CreateChild("child");

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Global)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventRegistry>()
                .Autowiring<EventRegistry>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventBroker>()
                .Autowiring<EventBroker>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Contract(typeof(IEvent<>))
                .KeyComparer(Wellknown.KeyComparer.AnyState)
                .Autowiring(typeof(Event<>));

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Contract<long>()
                .Tag("IdGenerator")
                .FactoryMethod(IdGenerator.GenerateId);

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventSource<DateTimeOffset>>()
                .Autowiring<TimerSource>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Contract(typeof(IEventListener<>))
                .Autowiring(typeof(ConsoleListener<>));

            yield return childContainer;
        }
    }
}