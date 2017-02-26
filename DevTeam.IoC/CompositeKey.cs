namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class CompositeKey: ICompositeKey
    {
        private static readonly Cache<IContractKey, ISet<IContractKey>> ContractSetCache = new Cache<IContractKey, ISet<IContractKey>>();
        private static readonly HashSet<IStateKey> EmptyStateKeys = new HashSet<IStateKey>();
        private static readonly HashSet<ITagKey> EmptyTagKeys = new HashSet<ITagKey>();
        private readonly int _contractsHashCode;
        private readonly int _tagsHashCode;
        private readonly int _statesHashCode;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKeys,
            [CanBeNull] IEnumerable<ITagKey> tagKeys = null,
            [CanBeNull] IEnumerable<IStateKey> stateKeys = null)
        {
            if (contractKeys == null) throw new ArgumentNullException(nameof(contractKeys));
            ContractKeys = CreateSet(contractKeys, out _contractsHashCode);
            TagKeys = tagKeys != null ? CreateSet(tagKeys, out _tagsHashCode) : EmptyTagKeys;
            StateKeys = stateKeys != null ? CreateSet(stateKeys, out _statesHashCode) : EmptyStateKeys;
        }

        public ISet<IContractKey> ContractKeys { get; }

        public ISet<ITagKey> TagKeys { get; }

        public ISet<IStateKey> StateKeys { get; }

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
            return $"{nameof(CompositeKey)} [Contracts: {string.Join(", ", ContractKeys)}, Tags: {string.Join(", ", TagKeys)}, States: {string.Join(", ", StateKeys)}]";
        }

        private bool Equals(ICompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                ContractKeys.SetEquals(other.ContractKeys)
                && (TagKeys.Count == 0 || filter.Filter(typeof(ITagKey)) || TagKeys.SetEquals(other.TagKeys))
                && (StateKeys.Count == 0 || filter.Filter(typeof(IStateKey)) || StateKeys.SetEquals(other.StateKeys));
        }

        private bool Equals(IContractKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                ContractKeys.Count == 1 && ContractKeys.Single().Equals(other)
                && (TagKeys.Count == 0 || filter.Filter(typeof(ITagKey)))
                && (StateKeys.Count == 0 || filter.Filter(typeof(IStateKey)));
        }

        [NotNull]
        private ISet<T> CreateSet<T>([NotNull] IEnumerable<T> keys, out int hashCode)
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