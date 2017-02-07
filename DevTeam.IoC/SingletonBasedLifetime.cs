namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class SingletonBasedLifetime<TKey> : ILifetime
    {
        private readonly Func<ILifetimeContext, IResolverContext, TKey> _keySelector;
        private readonly Dictionary<TKey, ILifetime> _lifitimes = new Dictionary<TKey, ILifetime>();

        public SingletonBasedLifetime([NotNull] Func<ILifetimeContext, IResolverContext, TKey> keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _keySelector = keySelector;
        }

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
            ILifetime lifetime;
            var key = _keySelector(lifetimeContext, resolverContext);
            lock (_lifitimes)
            {
                if (!_lifitimes.TryGetValue(key, out lifetime))
                {
                    lifetime = new SingletonLifetime();
                    _lifitimes.Add(key, lifetime);
                }
            }

            return lifetime.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
            
        }

        public void Dispose()
        {
            lock (_lifitimes)
            {
                foreach (var lifetime in _lifitimes)
                {
                    lifetime.Value.Dispose();
                }

                _lifitimes.Clear();
            }
        }
    }
}
