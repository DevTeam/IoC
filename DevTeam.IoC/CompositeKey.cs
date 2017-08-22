namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal sealed class CompositeKey: ICompositeKey
    {
        private static readonly Cache<IContractKey, HashSet<IContractKey>> ContractSetCache = new Cache<IContractKey, HashSet<IContractKey>>();
        private static readonly KeySet<IStateKey> EmptyStateKeys = new KeySet<IStateKey>();
        private static readonly KeySet<ITagKey> EmptyTagKeys = new KeySet<ITagKey>();
        private readonly int _contractsHashCode;
        private readonly int _statesHashCode;
        private readonly IKeySet<IContractKey> _contractKeys;
        private readonly IKeySet<ITagKey> _tagKeys;
        private readonly IKeySet<IStateKey> _stateKeys;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKeys,
            [CanBeNull] IEnumerable<ITagKey> tagKeys = null,
            [CanBeNull] IEnumerable<IStateKey> stateKeys = null)
        {
#if DEBUG
            if (contractKeys == null) throw new ArgumentNullException(nameof(contractKeys));
#endif
            _contractKeys = CreateSet(contractKeys, out _contractsHashCode);
            _tagKeys = tagKeys != null ? new KeySet<ITagKey>(tagKeys) : EmptyTagKeys;
            _stateKeys = stateKeys != null ? CreateSet(stateKeys, out _statesHashCode) : EmptyStateKeys;
        }

        public IKeySet<IContractKey> ContractKeys => _contractKeys;

        public IKeySet<ITagKey> TagKeys => _tagKeys;

        public IKeySet<IStateKey> StateKeys => _stateKeys;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj is IContractKey contractKey)
            {
                return Equals(contractKey);
            }

            var compositeKey = obj as ICompositeKey;
            return compositeKey != null && Equals(compositeKey);
        }

        public override int GetHashCode()
        {
            var filter = KeyFilterContext.Current;
            var hashCode = _contractsHashCode;
            unchecked
            {
                if (_statesHashCode != 0 && !filter.Filter(typeof(IStateKey)))
                {
                    hashCode = (hashCode*397) ^ _statesHashCode;
                }
            }

            return hashCode;
        }

        public override string ToString()
        {
            return $"{nameof(CompositeKey)} [Contracts: {string.Join(", ", _contractKeys.Select(i => i.ToString()).ToArray())}, Tags: {string.Join(", ", _tagKeys.Select(i => i.ToString()).ToArray())}, States: {string.Join(", ", _stateKeys.Select(i => i.ToString()).ToArray())}]";
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(ICompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.IsEqual(other.ContractKeys)
                && ((_tagKeys.Count == 0 && other.TagKeys.Count == 0) || filter.Filter(typeof(ITagKey)) || _tagKeys.IsIntersecting(other.TagKeys))
                && ((_stateKeys.Count == 0 && other.StateKeys.Count == 0) || filter.Filter(typeof(IStateKey)) || _stateKeys.IsEqual(other.StateKeys));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IContractKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.Count == 1 && _contractKeys.Single().Equals(other)
                && (_tagKeys.Count == 0 || filter.Filter(typeof(ITagKey)))
                && (_stateKeys.Count == 0 || filter.Filter(typeof(IStateKey)));
        }

        [NotNull]
        private IKeySet<T> CreateSet<T>([NotNull] IEnumerable<T> keys, out int hashCode)
            where T: IKey
        {
            var resultSet = new KeySet<T>(keys);
            hashCode = 0;
            foreach (var key in resultSet)
            {
                unchecked
                {
                    hashCode += key.GetHashCode();
                }
            }

            return resultSet;
        }

        private class KeySet<T> : HashSet<T>, IKeySet<T>
            where T : IKey
        {
            public KeySet()
            {
            }

            public KeySet(IEnumerable<T> keys)
                :base(keys)
            {
            }

            public bool IsEqual(IKeySet<T> keys)
            {
                return base.SetEquals(keys);
            }

            public bool IsIntersecting(IKeySet<T> keys)
            {
                return Enumerable.Intersect(this, keys).Any();
            }
        }
    }
}