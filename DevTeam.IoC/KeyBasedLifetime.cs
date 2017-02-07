namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class KeyBasedLifetime<TKey> : ILifetime
    {
        private readonly Func<ILifetimeContext, IResolverContext, TKey> _keySelector;
        private readonly Func<ILifetime> _baseLifetimeFactory;
        private readonly Dictionary<TKey, ILifetime> _lifetimes = new Dictionary<TKey, ILifetime>();

        internal int Count => _lifetimes.Count;

        public KeyBasedLifetime(
            [NotNull] Func<ILifetimeContext, IResolverContext, TKey> keySelector,
            [NotNull] Func<ILifetime> baseLifetimeFactory)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (baseLifetimeFactory == null) throw new ArgumentNullException(nameof(baseLifetimeFactory));
            _keySelector = keySelector;
            _baseLifetimeFactory = baseLifetimeFactory;
        }

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
            ILifetime lifetime;
            var key = _keySelector(lifetimeContext, resolverContext);
            lock (_lifetimes)
            {
                if (!_lifetimes.TryGetValue(key, out lifetime))
                {
                    lifetime = _baseLifetimeFactory();
                    _lifetimes.Add(key, lifetime);
                }
            }

            return lifetime.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
            
        }

        public void Dispose()
        {
            lock (_lifetimes)
            {
                foreach (var lifetime in _lifetimes)
                {
                    lifetime.Value.Dispose();
                }

                _lifetimes.Clear();
            }
        }
    }
}
