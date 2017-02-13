namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class CacheTracker: IObserver<IEventRegistration>
    {
        private readonly ICache<ICompositeKey, IResolverContext> _cache;

        public CacheTracker([NotNull] ICache<ICompositeKey, IResolverContext> cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            _cache = cache;
        }

        public void OnNext(IEventRegistration value)
        {
            if (value.Stage == EventStage.After && value.Action == RegistrationAction.Remove)
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