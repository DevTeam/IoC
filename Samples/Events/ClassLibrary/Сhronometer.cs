namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Сhronometer: IEventConsumer<DateTimeOffset>
    {
        private readonly IConsole _console;

        public Сhronometer(IConsole console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            _console = console;
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
    }
}
