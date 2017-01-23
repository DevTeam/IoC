namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal class Console : IConsole
    {
        private readonly bool _isTraceEnabled;
        private readonly ITrace _trace;

        public Console(IProvider<ITrace> traceProvider)
        {
            if (traceProvider == null) throw new ArgumentNullException(nameof(traceProvider));
            _isTraceEnabled = traceProvider.TryGet(out _trace);
        }

        public void WriteLine(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (_isTraceEnabled)
            {
                _trace.Output.Add(message);
            }
        }
    }
}