namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;

    internal class Timer: IEventProducer<DateTimeOffset>
    {
        private readonly ILogger _logger;
        private readonly List<IObserver<DateTimeOffset>> _observers = new List<IObserver<DateTimeOffset>>();

        public Timer(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _logger.LogInfo(this, "created");
        }

        public IDisposable Subscribe(IObserver<DateTimeOffset> observer)
        {
            _logger.LogInfo(this, $"Subscribe to {observer}");
            _observers.Add(observer);
            var cancellationTokenSource = new CancellationTokenSource();
            var task = Run(observer, cancellationTokenSource.Token);
            return new Subscription(() =>
            {
                _observers.Remove(observer);
                _logger.LogInfo(this, $"Unsubscribe from {observer}");

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
            return $"{nameof(Timer)}-{GetHashCode()}";
        }

        private static async Task Run(IObserver<DateTimeOffset> observer, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ContinueWith((task, state) =>
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
