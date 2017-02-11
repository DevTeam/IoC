namespace ClassLibrary
{
    using System;

    internal class Event<T>
    {
        private readonly ILogger<Event<T>> _logger;

        public Event(
            ILogger<Event<T>> logger,
            T data)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            Data = data;
        }

        public T Data { get; }

        public override string ToString()
        {
            return $"{_logger.InstanceName} {Data}";
        }
    }
}
