namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;

    internal class EventRegistry : IEventRegistry, IDisposable
    {
        private readonly IResolver _resolver;
        private readonly IEventBroker _eventBroker;
        private readonly List<IDisposable> _tokens = new List<IDisposable>();
        private readonly ILog _log;

        public EventRegistry(
            [Contract] Task<IResolver> resolver,
            [Contract] Task<IEventBroker> eventBroker,
            [State(0, typeof(string))] [Contract] Task<IResolver<string, ILog>> logResolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (eventBroker == null) throw new ArgumentNullException(nameof(eventBroker));
            if (logResolver == null) throw new ArgumentNullException(nameof(logResolver));

            logResolver.Start();
            resolver.Start();
            eventBroker.Start();

            logResolver.Wait();
            _log = logResolver.Result.Resolve(nameof(EventRegistry));
            _log.Method("Ctor()");
            resolver.Wait();
            _resolver = resolver.Result;
            eventBroker.Wait();
            _eventBroker = eventBroker.Result;
        }

        public void RegisterEvent<TEvent>()
        {
            _log.Method($"RegisterEvent<{typeof(TEvent).Name}>()");
            var sources = _resolver.Resolve().Instance<IEnumerable<IEventSource<TEvent>>>();
            var listners = _resolver.Resolve().Instance<IEnumerable<IEventListener<TEvent>>>();
            var tokens = 
                listners.Select(i => _eventBroker.RegisterListener(i))
                .Concat(sources.Select(i => _eventBroker.RegisterSource(i)))
                .ToList();

            _tokens.AddRange(tokens);
        }

        public void Dispose()
        {
            _log.Method("Dispose()");
            foreach (var token in _tokens)
            {
                token.Dispose();
            }

            _tokens.Clear();
        }

        public override string ToString()
        {
            return $"{nameof(EventRegistry)} [Tokens Count: {_tokens.Count}]";
        }
    }
}