namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Logger: ILogger
    {
        private readonly IConsole _console;

        public Logger(IConsole console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            _console = console;
        }

        public void LogInfo(object source, string info)
        {
            _console.WriteLine($"LOG {source}: {info}");
        }
    }
}
