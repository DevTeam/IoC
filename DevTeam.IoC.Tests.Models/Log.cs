namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal class Log : ILog
    {
        private readonly IConsole _console;
        private readonly string _name;

        public Log([Contract] IConsole console, string name)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            _console = console;
            _name = name;
        }

        public void Method(string description)
        {
            _console.WriteLine($"METHOD: {_name}.{description}");
        }
    }
}