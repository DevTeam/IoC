// ReSharper disable RedundantUsingDirective
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
        private int _tagKeysCount;
        private int _stateKeysCount;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKeys,
            [CanBeNull] IEnumerable<ITagKey> tagKeys = null,
            [CanBeNull] IEnumerable<IStateKey> stateKeys = null)
        {
#if DEBUG
            if (contractKeys == null) throw new ArgumentNullException(nameof(contractKeys));
#endif
            _contractKeys = new KeySet<IContractKey>(contractKeys);
            _contractsHashCode = _contractKeys.GetHashCode();
            _tagKeys = tagKeys != null ? new KeySet<ITagKey>(tagKeys) : EmptyTagKeys;
            _stateKeys = stateKeys != null ? new KeySet<IStateKey>(stateKeys) : EmptyStateKeys;
            _statesHashCode = _stateKeys.GetHashCode();
            _tagKeysCount = _tagKeys.Count;
            _stateKeysCount = _stateKeys.Count;
        }

        public IKeySet<IContractKey> ContractKeys => _contractKeys;

        public IKeySet<ITagKey> TagKeys => _tagKeys;

        public IKeySet<IStateKey> StateKeys => _stateKeys;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            switch (obj)
            {
                case CompositeKey compositeKey:
                    return Equals(compositeKey);

                case ICompositeKey compositeKey:
                    return Equals(compositeKey);

                case IContractKey contractKey:
                    return Equals(contractKey);

                default: throw new ContainerException($"Ivalid ${obj} to compare");
            }
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
        private bool Equals(CompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.Equals(other.ContractKeys)
                && ((_tagKeysCount == 0 && other._tagKeysCount == 0) || filter.Filter(typeof(ITagKey)) || (_tagKeysCount > 0 && other._tagKeysCount > 0 && _tagKeys.Intersect(other._tagKeys).Any()))
                && ((_stateKeysCount == 0 && other._stateKeysCount == 0) || filter.Filter(typeof(IStateKey)) || _stateKeys.Equals(other._stateKeys));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(ICompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.Equals(other.ContractKeys)
                && ((_tagKeysCount == 0 && other.TagKeys.Count == 0) || filter.Filter(typeof(ITagKey)) || (_tagKeysCount > 0 && other.TagKeys.Count > 0 && _tagKeys.Intersect(other.TagKeys).Any()))
                && ((_stateKeysCount == 0 && other.StateKeys.Count == 0) || filter.Filter(typeof(IStateKey)) || _stateKeys.Equals(other.StateKeys));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IContractKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.Count == 1 && _contractKeys.Single().Equals(other)
                && (_tagKeysCount == 0 || filter.Filter(typeof(ITagKey)))
                && (_stateKeysCount == 0 || filter.Filter(typeof(IStateKey)));
        }


        private class KeySet<T> : HashSet<T>, IKeySet<T>
            where T : IKey
        {
            private readonly int _hashCode;

            public KeySet()
            {
            }

            public KeySet(IEnumerable<T> keys)
                :base(keys)
            {
                foreach (var key in this)
                {
                    unchecked
                    {
                        _hashCode += key.GetHashCode();
                    }
                }
            }

            public bool Equals(IKeySet<T> keys)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return SetEquals(keys);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}