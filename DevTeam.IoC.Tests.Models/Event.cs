namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal sealed class Event<T> : IEvent<T>
    {
        public Event(
            [NotNull] [State(0, typeof(string), Value = nameof(Event<T>))] ILog log,
            [State] T data,
            [Tag("IdGenerator")] long id)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            log.Method("Ctor()");
            Data = data;
            Id = id;
        }

        public long Id { get; }

        public T Data { get; }
    }
}
