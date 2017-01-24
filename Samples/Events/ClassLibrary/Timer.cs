namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;

    internal class Timer: IEventProducer<DateTimeOffset>
    {
        private readonly IName<Timer> _name;
        private readonly ILogger _logger;
        private readonly TimeSpan _period;
        private readonly List<IObserver<DateTimeOffset>> _observers = new List<IObserver<DateTimeOffset>>();

        public Timer(
            IName<Timer> name,
            ILogger logger,
            TimeSpan period)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _name = name;
            _logger = logger;
            _period = period;
            _logger.LogInfo(_name, "created");
        }

        public IDisposable Subscribe(IObserver<DateTimeOffset> observer)
        {
            _logger.LogInfo(_name, $"Subscribe to {observer}");
            _observers.Add(observer);
            var cancellationTokenSource = new CancellationTokenSource();
            var task = Run(observer, cancellationTokenSource.Token);
            return new Subscription(() =>
            {
                _observers.Remove(observer);
                _logger.LogInfo(_name, $"Unsubscribe from {observer}");

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
            return _name.Short;
        }

        private async Task Run(IObserver<DateTimeOffset> observer, CancellationToken cancellationToken)
        {
            await Task.Delay(_period, cancellationToken).ContinueWith((task, state) =>
                {
                    try
                    {
                        observer.OnNext(DateTimeOffset.Now);
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
