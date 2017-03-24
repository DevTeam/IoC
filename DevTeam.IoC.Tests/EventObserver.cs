namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class EventObserver<T> : IObserver<T>
    {
        public IList<Event> Events { get; } = new List<Event>();

        public void OnNext(T value)
        {
            Events.Add(new Event(EventType.OnNext, value));
        }

        public void OnError(Exception error)
        {
            Events.Add(new Event(EventType.OnError, default(T), error));
        }

        public void OnCompleted()
        {
            Events.Add(new Event(EventType.OnCompleted, default(T)));
        }

        internal sealed class Event
        {
            public Event(EventType eventType, [CanBeNull] T value, [CanBeNull] Exception error = null)
            {
                EventType = eventType;
                Value = value;
                Error = error;
            }

            public EventType EventType { get; }

            [CanBeNull]
            public T Value { get; }

            [CanBeNull]
            // ReSharper disable once MemberCanBePrivate.Global
            public Exception Error { get; }
        }

        internal enum EventType
        {
            OnNext,

            OnError,

            OnCompleted
        }
    }
}
