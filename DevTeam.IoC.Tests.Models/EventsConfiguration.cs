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

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver.Feature(Wellknown.Feature.Default);
            yield return new PlatformConfiguration(_trace);
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var childContainer = resolver.CreateChild("child");

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Global)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventRegistry>()
                .AsAutowiring<EventRegistry>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventBroker>()
                .AsAutowiring<EventBroker>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Contract(typeof(IEvent<>))
                .KeyComparer(Wellknown.KeyComparer.AnyState)
                .AsAutowiring(typeof(Event<>));

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Contract<long>()
                .Tag("IdGenerator")
                .AsFactoryMethod(IdGenerator.GenerateId);

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IEventSource<DateTimeOffset>>()
                .AsAutowiring<TimerSource>();

            yield return childContainer
                .Register()
                .Scope(Wellknown.Scope.Internal)
                .Lifetime(Wellknown.Lifetime.PerResolve)
                .Contract(typeof(IEventListener<>))
                .AsAutowiring(typeof(ConsoleListener<>));

            yield return childContainer;
        }
    }
}