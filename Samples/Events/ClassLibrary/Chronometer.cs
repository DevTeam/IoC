namespace ClassLibrary
{
    using System;
    using Contracts;

    // ReSharper disable once UnusedMember.Global
    internal sealed class Chronometer: IEventConsumer<DateTimeOffset>
    {
        private readonly IConsole _console;
        private readonly ILogger<Chronometer> _logger;
        private int _tickCount;

        public Chronometer(
            IConsole console,
            ILogger<Chronometer> logger)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public void OnNext(DateTimeOffset value)
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
