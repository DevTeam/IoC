namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class Timer : ITimer, ITimerManager
    {
        private readonly ILog _log;
        private readonly List<Action> _tickActions = new List<Action>();

        public Timer([State(0, typeof(string), Value = nameof(Timer))] ILog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            _log = log;
            _log.Method("Ctor()");
        }

        public void Tick()
        {
            _log.Method("Tick()");
            foreach (var action in _tickActions)
            {
                action();
            }
        }

        public IDisposable Subscribe(Action tickAction)
        {
            _log.Method("Subscribe()");
            _tickActions.Add(tickAction);
            return new Subscription(() => _tickActions.Remove(tickAction));
        }
    }
}