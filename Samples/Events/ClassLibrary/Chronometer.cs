namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Chronometer: IEventConsumer<DateTimeOffset>
    {
        private readonly IName<Chronometer> _name;
        private readonly IConsole _console;
        private int _tickCount;

        public Chronometer(
            IName<Chronometer> name,
            IConsole console,
            ILogger logger)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (console == null) throw new ArgumentNullException(nameof(console));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _name = name;
            _console = console;
            logger.LogInfo(name, "created");
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
            return _name.Short;
        }
    }
}
