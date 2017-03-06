namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class CacheTracker: IObserver<IRegistrationEvent>
    {
        private readonly ICache<IKey, IResolverContext> _cache;

        public CacheTracker([NotNull] ICache<IKey, IResolverContext> cache)
        {
#if DEBUG
            if (cache == null) throw new ArgumentNullException(nameof(cache));
#endif
            _cache = cache;
        }

        public void OnNext(IRegistrationEvent value)
        {
            if (value.Stage == EventStage.After)
            {
                _cache.TryRemove(value.Key);
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}