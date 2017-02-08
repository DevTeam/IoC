namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class CompositeKey: ICompositeKey
    {
        private readonly int _baseHashCode;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKey,
            [NotNull] IEnumerable<ITagKey> tagKeys,
            [NotNull] IEnumerable<IStateKey> stateKeys)
        {
            if (contractKey == null) throw new ArgumentNullException(nameof(contractKey));
            if (tagKeys == null) throw new ArgumentNullException(nameof(tagKeys));
            if (stateKeys == null) throw new ArgumentNullException(nameof(stateKeys));
            var tagKeysGroupedByType = (
                from tagKey in tagKeys
                group tagKey by tagKey.Tag.GetType()).OrderBy(i => i.Key.FullName);
            TagKeys = tagKeysGroupedByType.Select(i => i.OrderBy(j => j.Tag)).SelectMany(i => i).ToArray();
            StateKeys = stateKeys.OrderBy(i => i.Index).ToArray();
            ContractKeys = contractKey.OrderBy(i => i.ContractType.FullName).ToArray();
            unchecked
            {
                _baseHashCode = ContractKeys.Aggregate(1, (code, key) =>
                {
                    unchecked
                    {
                        return (code*397) ^ key.GetHashCode();
                    }
                });
            }
        }

        public IContractKey[] ContractKeys { get; }

        public ITagKey[] TagKeys { get; }

        public IStateKey[] StateKeys { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var key = obj as CompositeKey;
            return key != null && Equals(key);
        }

        public override int GetHashCode()
        {
            var filter = KeyFilterContext.Current;
            var hashCode = _baseHashCode;
            if (!filter.Filter(typeof(ITagKey)))
            {
                hashCode = TagKeys.Aggregate(hashCode, (code, key) =>
                {
                    unchecked
                    {
                        return (code*397) ^ key.GetHashCode();
                    }
                });
            }

            if (!filter.Filter(typeof(IStateKey)))
            {
                hashCode = StateKeys.Aggregate(hashCode, (code, key) =>
                {
                    unchecked
                    {
                        return (code*397) ^ key.GetHashCode();
                    }
                });
            }

            return hashCode;
        }

        private bool Equals(CompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                ContractKeys.Length == other.ContractKeys.Length
                && ContractKeys.SequenceEqual(other.ContractKeys)
                && (filter.Filter(typeof(ITagKey)) || TagKeys.SequenceEqual(other.TagKeys))
                && (filter.Filter(typeof(IStateKey)) || StateKeys.SequenceEqual(other.StateKeys));
        }

        public override string ToString()
        {
            return $"{nameof(CompositeKey)} [Contracts: {string.Join(", ", (IEnumerable<IContractKey>)ContractKeys)}, Tags: {string.Join(", ", (IEnumerable<ITagKey>)TagKeys)}, States: {string.Join(", ", (IEnumerable<IStateKey>)StateKeys)}]";
        }
    }
}