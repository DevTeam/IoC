namespace ClassLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;

    internal class Timer: IEventProducer<DateTimeOffset>
    {
        private readonly ILogger<Timer> _logger;
        private readonly TimeSpan _period;
        private readonly List<IObserver<DateTimeOffset>> _observers = new List<IObserver<DateTimeOffset>>();

        public Timer(
            ILogger<Timer> logger,
            TimeSpan period)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _period = period;
            _logger.LogInfo("created");
        }

        public IDisposable Subscribe(IObserver<DateTimeOffset> observer)
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
