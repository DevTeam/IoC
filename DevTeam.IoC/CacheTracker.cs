namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class CacheTracker: IObserver<IRegistrationEvent>
    {
        private readonly ICache<ICompositeKey, IResolverContext> _cache;

        public CacheTracker([NotNull] ICache<ICompositeKey, IResolverContext> cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            _cache = cache;
        }

        public void OnNext(IRegistrationEvent value)
        {
            if (value.Stage == EventStage.After && value.Action == EventAction.Remove)
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