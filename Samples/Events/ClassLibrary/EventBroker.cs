namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class EventBroker<T>: IEventBroker, IDisposable
    {
        private readonly IEnumerable<IDisposable> _subsriptions;

        public EventBroker(
            IEnumerable<IEventProducer<T>> eventProducers,
            IEnumerable<IEventConsumer<T>> eventConsumers)
        {
            if (eventProducers == null) throw new ArgumentNullException(nameof(eventProducers));
            if (eventConsumers == null) throw new ArgumentNullException(nameof(eventConsumers));
            _subsriptions = new List<IDisposable>(
                from eventConsumer in eventConsumers
                from eventProducer in eventProducers
                select CreateSubscription(eventProducer, eventConsumer)
            );
        }

        public void Dispose()
        {
            foreach (var subsription in _subsriptions)
            {
                subsription.Dispose();
            }
        }

        private static IDisposable CreateSubscription(IEventProducer<T> eventProducer, IEventConsumer<T> eventConsumer)
        {
            return eventProducer.Subscribe(eventConsumer);
        }
    }
}