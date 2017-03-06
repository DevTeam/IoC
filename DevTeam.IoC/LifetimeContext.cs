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
        private long? _resolveId;

        public LifetimeContext()
        {
            _previous = _current;
            _current = this;
        }

        public long ResolveId {
            get
            {
                if (_previous != null)
                {
                    return _previous.ResolveId;
                }

                if (_resolveId == null)
                {
                    _resolveId = GenerateId();
                }

                return _resolveId.Value;
            }
        }

        // ReSharper disable once PossibleInvalidOperationException
        public long ThreadId
        {
            get
            {
                if (_threadId == null)
                {
                    _threadId = GenerateId();
                }

                return _threadId.Value;
            }
        }

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
