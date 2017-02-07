namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class Cache<TKey, TValue> : ICache<TKey, TValue>
        where TValue: class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _cache = new Dictionary<TKey, WeakReference<TValue>>();

        public bool TryGet(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            WeakReference<TValue> weakReference;
            if (_cache.TryGetValue(key, out weakReference))
            {
                if (weakReference.TryGetTarget(out value))
                {
                    return true;
                }

                _cache.Remove(key);
            }

            value = default(TValue);
            return false;
        }

        public void Set(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _cache[key] = new WeakReference<TValue>(value);
        }

        public bool TryRemove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _cache.Remove(key);
        }
    }
}
