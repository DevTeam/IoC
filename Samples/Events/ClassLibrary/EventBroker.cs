namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class EventBroker<T>: IEventBroker, IDisposable
    {
        private readonly IEnumerable<IDisposable> _subsriptions;
        private readonly ILogger<EventBroker<T>> _logger;

        public EventBroker(
            ILogger<EventBroker<T>> logger,
            IEnumerable<IEventProducer<T>> eventProducers,
            IEnumerable<IEventConsumer<T>> eventConsumers)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventProducers == null) throw new ArgumentNullException(nameof(eventProducers));
            if (eventConsumers == null) throw new ArgumentNullException(nameof(eventConsumers));
            _logger = logger;
            _logger.LogInfo("creating");
            _subsriptions = new List<IDisposable>(
                from eventConsumer in eventConsumers
                from eventProducer in eventProducers
                select CreateSubscription(eventProducer, eventConsumer)
            );
            _logger.LogInfo("created");
        }

        public void Dispose()
        {
            _logger.LogInfo("disposing");
            foreach (var subsription in _subsriptions)
            {
                subsription.Dispose();
            }

            _logger.LogInfo("disposed");
        }

        public override string ToString()
        {
            return _logger.InstanceName;
        }

        private static IDisposable CreateSubscription(IObservable<T> eventProducer, IObserver<T> eventConsumer)
        {
            return eventProducer.Subscribe(eventConsumer);
        }
    }
}