namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class Cache<TKey, TValue> : ICache<TKey, TValue>
        where TValue: class
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        internal int Count => _cache.Count;

        protected IEnumerable<TValue> Values => _cache.Values;

        public bool TryGet(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_cache.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        public void Set(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _cache[key] = value;
        }

        public bool TryRemove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _cache.Remove(key);
        }
    }
}
