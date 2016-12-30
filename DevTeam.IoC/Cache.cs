namespace DevTeam.IoC
{
    using System.Collections.Generic;
    using Contracts;

    internal class Cache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        public bool TryGet(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Set(TKey key, TValue value)
        {
            _cache[key] = value;
        }

        public bool TryRemove(TKey key)
        {
            return _cache.Remove(key);
        }
    }
}
