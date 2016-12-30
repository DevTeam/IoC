namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class TimerSource: IEventSource<DateTimeOffset>, IDisposable
    {
        private readonly ILog _log;
        private readonly IResolver<DateTimeOffset, IEvent<DateTimeOffset>> _eventResolver;
        private readonly IDisposable _timerSubscription;
        private readonly List<IEventListener<DateTimeOffset>> _listeners = new List<IEventListener<DateTimeOffset>>();

        public TimerSource(
            [Contract] ITimer timer,
            [State(0, typeof(string), Value = nameof(TimerSource))] [Contract] ILog log,
            [Contract] IResolver<DateTimeOffset, IEvent<DateTimeOffset>> eventResolver)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (eventResolver == null) throw new ArgumentNullException(nameof(eventResolver));
            log.Method("Ctor()");
            _timerSubscription = timer.Subscribe(Tick);
            _log = log;
            _eventResolver = eventResolver;
        }

        public IDisposable Subscribe(IEventListener<DateTimeOffset> listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            _log.Method($"Subscribe({listener})");
            _listeners.Add(listener);
            return new Subscription(() =>
                {
                    _log.Method($"Unsubscribe({listener})");
                    _listeners.Remove(listener);
                });
        }

        public void Dispose()
        {
            _log.Method("Dispose()");
            _timerSubscription.Dispose();
        }

        private void Tick()
        {
            var e = _eventResolver.Resolve(DateTimeOffset.Now);
            _log.Method("Tick()");
            foreach (var listener in _listeners)
            {
                listener.OnEvent(e);
            }
        }

        public override string ToString()
        {
            return $"{nameof(TimerSource)} [Listeners: {string.Join(", ", _listeners)}]";
        }
    }
}
