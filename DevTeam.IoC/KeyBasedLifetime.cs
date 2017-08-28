namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal abstract class KeyBasedLifetime<TKey> : ILifetime
    {
        private readonly Func<ILifetimeContext, CreationContext, TKey> _keySelector;
        private readonly Dictionary<TKey, ILifetime> _lifetimes = new Dictionary<TKey, ILifetime>();

        internal int Count {
            get
            {
                lock (_lifetimes)
                {
                    return _lifetimes.Count;
                }
            }
        }

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        protected KeyBasedLifetime(
            [NotNull] Func<ILifetimeContext, CreationContext, TKey> keySelector)
        {
#if DEBUG
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
#endif
            _keySelector = keySelector;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public object Create(ILifetimeContext lifetimeContext, CreationContext creationContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
#if DEBUG
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
#endif
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

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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
