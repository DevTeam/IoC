namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Chronometer: IEventConsumer<DateTimeOffset>
    {
        private readonly IConsole _console;
        private readonly ILogger _logger;

        public Chronometer(
            IConsole console,
            ILogger logger)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _console = console;
            _logger = logger;
            _logger.LogInfo(this, "created");
        }

        public void OnCompleted()
        {
            _console.WriteLine("Finish");
        }

        public void OnError(Exception error)
        {
            _console.WriteLine($"Error {error}");
        }

        public void OnNext(DateTimeOffset value)
        {
            _console.WriteLine($"Time {value}");
        }

        public override string ToString()
        {
            return $"{nameof(Chronometer)}-{GetHashCode()}";
        }
    }
}
