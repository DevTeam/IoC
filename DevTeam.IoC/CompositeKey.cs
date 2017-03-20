namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class CompositeKey: ICompositeKey
    {
        private static readonly Cache<IContractKey, HashSet<IContractKey>> ContractSetCache = new Cache<IContractKey, HashSet<IContractKey>>();
        private static readonly HashSet<IStateKey> EmptyStateKeys = new HashSet<IStateKey>();
        private static readonly HashSet<ITagKey> EmptyTagKeys = new HashSet<ITagKey>();
        private readonly int _contractsHashCode;
        private readonly int _tagsHashCode;
        private readonly int _statesHashCode;
        private readonly HashSet<IContractKey> _contractKeys;
        private readonly HashSet<ITagKey> _tagKeys;
        private readonly HashSet<IStateKey> _stateKeys;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKeys,
            [CanBeNull] IEnumerable<ITagKey> tagKeys = null,
            [CanBeNull] IEnumerable<IStateKey> stateKeys = null)
        {
#if DEBUG
            if (contractKeys == null) throw new ArgumentNullException(nameof(contractKeys));
#endif
            _contractKeys = CreateSet(contractKeys, out _contractsHashCode);
            _tagKeys = tagKeys != null ? CreateSet(tagKeys, out _tagsHashCode) : EmptyTagKeys;
            _stateKeys = stateKeys != null ? CreateSet(stateKeys, out _statesHashCode) : EmptyStateKeys;
        }

        public IEnumerable<IContractKey> ContractKeys => _contractKeys;

        public IEnumerable<ITagKey> TagKeys => _tagKeys;

        public IEnumerable<IStateKey> StateKeys => _stateKeys;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var contractKey = obj as IContractKey;
            if (contractKey != null)
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
                if (_tagsHashCode != 0 && !filter.Filter(typeof(ITagKey)))
                {
                    hashCode = (hashCode*397) ^ _tagsHashCode;
                }

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

        private bool Equals(ICompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.SetEquals(other.ContractKeys)
                && (_tagKeys.Count == 0 || filter.Filter(typeof(ITagKey)) || _tagKeys.SetEquals(other.TagKeys))
                && (_stateKeys.Count == 0 || filter.Filter(typeof(IStateKey)) || _stateKeys.SetEquals(other.StateKeys));
        }

        private bool Equals(IContractKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                _contractKeys.Count == 1 && _contractKeys.Single().Equals(other)
                && (_tagKeys.Count == 0 || filter.Filter(typeof(ITagKey)))
                && (_stateKeys.Count == 0 || filter.Filter(typeof(IStateKey)));
        }

        [NotNull]
        private HashSet<T> CreateSet<T>([NotNull] IEnumerable<T> keys, out int hashCode)
        {
            var resultSet = new HashSet<T>(keys);
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
    }
}