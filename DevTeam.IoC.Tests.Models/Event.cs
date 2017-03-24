namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal sealed class Event<T> : IEvent<T>
    {
        [NotNull] private readonly ILog _log;

        public Event(
            [NotNull] [State(0, typeof(string), Value = nameof(Event<T>))] ILog log,
            [State] T data,
            [Tag("IdGenerator")] long id)
        {
            _log = log;
            if (log == null) throw new ArgumentNullException(nameof(log));
            log.Method("Ctor()");
        }

        public long Id { get; private set; }

        public T Data { get; private set; }
    }
}
