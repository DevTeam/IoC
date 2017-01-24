namespace ClassLibrary
{
    using System;
    using Contracts;

    internal class Logger<T>: ILogger<T>
    {
        private readonly IName<T> _name;
        private readonly IConsole _console;

        public Logger(
            IName<T> name,
            IConsole console)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (console == null) throw new ArgumentNullException(nameof(console));
            _name = name;
            _console = console;
        }

        public string InstanceName => _name.Short;

        public void LogInfo(string info)
        {
            _console.WriteLine($"LOG {InstanceName}: {info}", Color.Log);
        }
    }
}
