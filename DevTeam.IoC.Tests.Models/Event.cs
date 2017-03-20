namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal class Event<T> : IEvent<T>
    {
        [NotNull] private readonly ILog _log;

        public Event([NotNull] [State(0, typeof(string), Value = nameof(Event<T>))] ILog log)
        {
            _log = log;
            if (log == null) throw new ArgumentNullException(nameof(log));
            log.Method("Ctor()");
        }

        [Autowiring]
        public object Initialize([State] T data, [Tag("IdGenerator")] long id)
        {
            Data = data;
            Id = id;
            _log.Method($"Initialize({data}, {id})");
            return true;
        }

        public IConsole Console
        {
            [Autowiring]
            // ReSharper disable once ValueParameterNotUsed
            set
            {
            }
        }

        public long Id { get; private set; }

        public T Data { get; private set; }
    }
}
