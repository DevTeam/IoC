namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal abstract class KeyBasedLifetime<TKey> : ILifetime
    {
        private readonly Func<ILifetimeContext, ICreationContext, TKey> _keySelector;
        private readonly Dictionary<TKey, ILifetime> _lifetimes = new Dictionary<TKey, ILifetime>();

        internal int Count => _lifetimes.Count;

        protected KeyBasedLifetime(
            [NotNull] Func<ILifetimeContext, ICreationContext, TKey> keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _keySelector = keySelector;
        }

        public object Create(ILifetimeContext lifetimeContext, ICreationContext creationContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
            ILifetime lifetime;
            var key = _keySelector(lifetimeContext, creationContext);
            lock (_lifetimes)
            {
                if (!_lifetimes.TryGetValue(key, out lifetime))
                {
                    lifetime = CreateBaseLifetime(lifetimeEnumerator);
                    _lifetimes.Add(key, lifetime);
                }
            }

            return lifetime.Create(lifetimeContext, creationContext, lifetimeEnumerator);
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

        protected abstract ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator);
    }
}
