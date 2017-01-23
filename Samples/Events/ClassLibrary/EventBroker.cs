namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class EventBroker<T>: IEventBroker, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IDisposable> _subsriptions;

        public EventBroker(
            ILogger logger,
            IEnumerable<IEventProducer<T>> eventProducers,
            IEnumerable<IEventConsumer<T>> eventConsumers)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventProducers == null) throw new ArgumentNullException(nameof(eventProducers));
            if (eventConsumers == null) throw new ArgumentNullException(nameof(eventConsumers));
            _logger = logger;
            _logger.LogInfo(this, "creating");
            _subsriptions = new List<IDisposable>(
                from eventConsumer in eventConsumers
                from eventProducer in eventProducers
                select CreateSubscription(eventProducer, eventConsumer)
            );
            _logger.LogInfo(this, "created");
        }

        public void Dispose()
        {
            _logger.LogInfo(this, "disposing");
            foreach (var subsription in _subsriptions)
            {
                subsription.Dispose();
            }

            _logger.LogInfo(this, "disposed");
        }

        public override string ToString()
        {
            return $"{nameof(EventBroker<T>)}-{GetHashCode()}";
        }

        private static IDisposable CreateSubscription(IObservable<T> eventProducer, IObserver<T> eventConsumer)
        {
            return eventProducer.Subscribe(eventConsumer);
        }
    }
}