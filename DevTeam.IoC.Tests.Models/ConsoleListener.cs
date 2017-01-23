namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal class ConsoleListener<TEvent>: IEventListener<TEvent>
    {
        private readonly IConsole _console;
        private readonly ILog _log;

        public ConsoleListener(
            IConsole console,
            [State(0, typeof(string), Value = nameof(ConsoleListener<TEvent>))] [Contract] ILog log)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            if (log == null) throw new ArgumentNullException(nameof(log));
            log.Method("Ctor()");
            _console = console;
            _log = log;
        }

        public void OnEvent(IEvent<TEvent> e)
        {
            _log.Method($"OnEvent({e})");
            _console.WriteLine($"OUTPUT: {e.Data}");
        }

        public override string ToString()
        {
            return $"{nameof(ConsoleListener<TEvent>)}";
        }
    }
}
