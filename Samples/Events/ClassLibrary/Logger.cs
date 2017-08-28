namespace ClassLibrary
{
    using System;
    using Contracts;

    // ReSharper disable once UnusedMember.Global
    internal sealed class Logger<T>: ILogger<T>
    {
        private readonly IName<T> _name;
        private readonly IConsole _console;

        public Logger(
            IName<T> name,
            IConsole console)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public string InstanceName => _name.Short;

        public void LogInfo(string info)
        {
            _console.WriteLine($"LOG {InstanceName}: {info}", Color.Log);
        }
    }
}
