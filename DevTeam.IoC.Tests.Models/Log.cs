namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    // to test ConfigurationFromAssembly
    [Contract(typeof(ILog))]
    [State(0, typeof(string))]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class Log : ILog
    {
        private readonly IConsole _console;
        private readonly string _name;

        public Log(
            [NotNull] IConsole console,
            [NotNull] [State] string name)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            if (name == null) throw new ArgumentNullException(nameof(name));
            _console = console;
            _name = name;
        }

        public void Method(string description)
        {
            _console.WriteLine($"METHOD: {_name}.{description}");
        }
    }
}