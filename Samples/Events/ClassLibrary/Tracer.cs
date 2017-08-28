namespace ClassLibrary
{
    using System;
    using Contracts;

    // ReSharper disable once UnusedMember.Global
    internal sealed class Tracer<T>: IEventConsumer<T>, IDisposable
    {
        private readonly ILogger<Tracer<T>> _logger;

        public Tracer(ILogger<Tracer<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logger.LogInfo("created");
        }

        public void OnCompleted()
        {
            System.Diagnostics.Debug.WriteLine("OnCompleted");
        }

        public void OnError(Exception error)
        {
            System.Diagnostics.Debug.WriteLine($"OnError {error}");
        }

        public void OnNext(T value)
        {
            System.Diagnostics.Debug.WriteLine($"OnNext {value}");
        }

        public override string ToString()
        {
            return _logger.InstanceName;
        }

        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("End of tracing");
        }
    }
}
