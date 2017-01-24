namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Chronometer: IEventConsumer<Event<DateTimeOffset>>
    {
        private readonly IConsole _console;
        private readonly ILogger<Chronometer> _logger;
        private int _tickCount;

        public Chronometer(
            IConsole console,
            ILogger<Chronometer> logger)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _console = console;
            _logger = logger;
            logger.LogInfo("created");
        }

        public void OnCompleted()
        {
            _console.WriteLine($"Finish after {_tickCount} ticks", Color.Normal);
        }

        public void OnError(Exception error)
        {
            _console.WriteLine($"Error {error}", Color.Error);
        }

        public void OnNext(Event<DateTimeOffset> value)
        {
            _tickCount++;
            _console.WriteLine($"Time {value}", Color.Normal);
        }

        public override string ToString()
        {
            return _logger.InstanceName;
        }
    }
}
