namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class Cache<TKey, TValue> : ICache<TKey, TValue>
        where TValue: class
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        internal int Count => _cache.Count;

        internal IEnumerable<TValue> Values => _cache.Values;

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool TryGet(TKey key, out TValue value)
        {
#if DEBUG
            if (key == null) throw new ArgumentNullException(nameof(key));
#endif
            if (_cache.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        // ReSharper disable once JoinNullCheckWithUsage
        public void Set(TKey key, TValue value)
        {
#if DEBUG
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
#endif
            _cache[key] = value;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool TryRemove(TKey key)
        {
#if DEBUG
            if (key == null) throw new ArgumentNullException(nameof(key));
#endif
            return _cache.Remove(key);
        }
    }
}
