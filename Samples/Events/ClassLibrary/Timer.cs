namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;

    internal class Timer: IEventProducer<Event<DateTimeOffset>>
    {
        private readonly ILogger<Timer> _logger;
        private readonly Func<DateTimeOffset, Event<DateTimeOffset>> _eventsFactory;
        private readonly TimeSpan _period;
        private readonly List<IObserver<Event<DateTimeOffset>>> _observers = new List<IObserver<Event<DateTimeOffset>>>();

        public Timer(
            ILogger<Timer> logger,
            Func<DateTimeOffset, Event<DateTimeOffset>> eventsFactory,
            TimeSpan period)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventsFactory == null) throw new ArgumentNullException(nameof(eventsFactory));
            _logger = logger;
            _eventsFactory = eventsFactory;
            _period = period;
            _logger.LogInfo("created");
        }

        public IDisposable Subscribe(IObserver<Event<DateTimeOffset>> observer)
        {
            _logger.LogInfo($"Subscribe to {observer}");
            _observers.Add(observer);
            var cancellationTokenSource = new CancellationTokenSource();
            var task = Run(observer, cancellationTokenSource.Token);
            return new Subscription(() =>
            {
                _observers.Remove(observer);
                _logger.LogInfo($"Unsubscribe from {observer}");

                cancellationTokenSource.Cancel();
                try
                {
                    task.Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }

                observer.OnCompleted();
            });
        }

        public override string ToString()
        {
            return _logger.InstanceName;
        }

        private async Task Run(IObserver<Event<DateTimeOffset>> observer, CancellationToken cancellationToken)
        {
            await Task.Delay(_period, cancellationToken).ContinueWith((task, state) =>
                {
                    try
                    {
                        observer.OnNext(_eventsFactory(DateTimeOffset.Now));
                        Run(observer, cancellationToken).Wait(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }, 
                null, cancellationToken);
        }

        private class Subscription : IDisposable
        {
            private readonly Action _remover;

            public Subscription(Action remover)
            {
                if (remover == null) throw new ArgumentNullException(nameof(remover));
                _remover = remover;
            }

            public void Dispose()
            {
                _remover();
            }
        }
    }
}
