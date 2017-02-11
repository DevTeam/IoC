namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class CompositeKey: ICompositeKey
    {
        private readonly int _contractsHashCode;
        private readonly int _tagsHashCode;
        private readonly int _statesHashCode;

        public CompositeKey(
            [NotNull] IEnumerable<IContractKey> contractKeys,
            [NotNull] IEnumerable<ITagKey> tagKeys,
            [NotNull] IEnumerable<IStateKey> stateKeys)
        {
            if (contractKeys == null) throw new ArgumentNullException(nameof(contractKeys));
            if (tagKeys == null) throw new ArgumentNullException(nameof(tagKeys));
            if (stateKeys == null) throw new ArgumentNullException(nameof(stateKeys));
            TagKeys = new HashSet<ITagKey>(tagKeys);
            StateKeys = new HashSet<IStateKey>(stateKeys);
            ContractKeys = new HashSet<IContractKey>(contractKeys);
            unchecked
            {
                _contractsHashCode = ContractKeys.Aggregate(1, (code, key) =>
                {
                    unchecked
                    {
                        return code + key.GetHashCode();
                    }
                }) * ContractKeys.Count;

                _tagsHashCode = TagKeys.Aggregate(1, (code, key) =>
                {
                    unchecked
                    {
                        return code + key.GetHashCode();
                    }
                }) * TagKeys.Count;

                _statesHashCode = StateKeys.Aggregate(1, (code, key) =>
                {
                    unchecked
                    {
                        return code + key.GetHashCode();
                    }
                }) * StateKeys.Count;
            }
        }

        public ISet<IContractKey> ContractKeys { get; }

        public ISet<ITagKey> TagKeys { get; }

        public ISet<IStateKey> StateKeys { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var key = obj as CompositeKey;
            return key != null && Equals(key);
        }

        public override int GetHashCode()
        {
            var filter = KeyFilterContext.Current;
            var hashCode = _contractsHashCode;
            unchecked
            {
                if (!filter.Filter(typeof(ITagKey)))
                {
                    hashCode = (hashCode*397) ^ _tagsHashCode;
                }

                if (!filter.Filter(typeof(IStateKey)))
                {
                    hashCode = (hashCode*397) ^ _statesHashCode;
                }
            }

            return hashCode;
        }

        private bool Equals(CompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                ContractKeys.SetEquals(other.ContractKeys)
                && (filter.Filter(typeof(ITagKey)) || TagKeys.SetEquals(other.TagKeys))
                && (filter.Filter(typeof(IStateKey)) || StateKeys.SetEquals(other.StateKeys));
        }

        public override string ToString()
        {
            return $"{nameof(CompositeKey)} [Contracts: {string.Join(", ", ContractKeys)}, Tags: {string.Join(", ", TagKeys)}, States: {string.Join(", ", StateKeys)}]";
        }
    }
}