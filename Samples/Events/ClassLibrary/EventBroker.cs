namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class EventBroker<T>: IEventBroker, IDisposable
    {
        private readonly IName<EventBroker<T>> _name;
        private readonly ILogger _logger;
        private readonly IEnumerable<IDisposable> _subsriptions;

        public EventBroker(
            IName<EventBroker<T>> name,
            ILogger logger,
            IEnumerable<IEventProducer<T>> eventProducers,
            IEnumerable<IEventConsumer<T>> eventConsumers)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventProducers == null) throw new ArgumentNullException(nameof(eventProducers));
            if (eventConsumers == null) throw new ArgumentNullException(nameof(eventConsumers));
            _name = name;
            _logger = logger;
            _logger.LogInfo(_name, "creating");
            _subsriptions = new List<IDisposable>(
                from eventConsumer in eventConsumers
                from eventProducer in eventProducers
                select CreateSubscription(eventProducer, eventConsumer)
            );
            _logger.LogInfo(_name, "created");
        }

        public void Dispose()
        {
            _logger.LogInfo(_name, "disposing");
            foreach (var subsription in _subsriptions)
            {
                subsription.Dispose();
            }

            _logger.LogInfo(_name, "disposed");
        }

        public override string ToString()
        {
            return _name.Short;
        }

        private static IDisposable CreateSubscription(IObservable<T> eventProducer, IObserver<T> eventConsumer)
        {
            return eventProducer.Subscribe(eventConsumer);
        }
    }
}