namespace DevTeam.IoC
{
    using System;
    using System.Threading;
    using Contracts;

    internal class LifetimeContext : IDisposable, ILifetimeContext
    {
        private static long _curId;
        private static LifetimeContext _current;
        [ThreadStatic] private static long? _threadId;
        private readonly LifetimeContext _previous;

        public LifetimeContext()
        {
            ResolveId = _current?.ResolveId ?? GenerateId();
            if (_threadId == null)
            {
                _threadId = GenerateId();
            }

            _previous = _current;
            _current = this;
        }

        public long ResolveId { get; }

        // ReSharper disable once PossibleInvalidOperationException
        public long ThreadId => _threadId.Value;

        public void Dispose()
        {
            _current = _previous;
        }

        private static long GenerateId()
        {
            return Interlocked.Increment(ref _curId);
        }
    }
}
