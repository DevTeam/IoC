namespace DevTeam.IoC.Tests.Models
{
    using Contracts;

    internal class Event<T> : IEvent<T>
    {
        public Event(T data, [Contract] [Tag("IdGenerator")] long id)
        {
            Data = data;
            Id = id;
        }

        public long Id { get; }

        public T Data { get; }
    }
}
