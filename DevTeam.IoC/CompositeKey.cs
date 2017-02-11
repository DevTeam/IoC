﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class CompositeKey: ICompositeKey
    {
        private static readonly HashSet<IContractKey> EmptyContractKeys = new HashSet<IContractKey>();
        private static readonly HashSet<IStateKey> EmptyStateKeys = new HashSet<IStateKey>();
        private static readonly HashSet<ITagKey> EmptyTagKeys = new HashSet<ITagKey>();
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
            ContractKeys = CreateSet(contractKeys, out _contractsHashCode) ?? EmptyContractKeys;
            TagKeys = CreateSet(tagKeys, out _tagsHashCode) ?? EmptyTagKeys;
            StateKeys = CreateSet(stateKeys, out _statesHashCode) ?? EmptyStateKeys;
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

        public override string ToString()
        {
            return $"{nameof(CompositeKey)} [Contracts: {string.Join(", ", ContractKeys)}, Tags: {string.Join(", ", TagKeys)}, States: {string.Join(", ", StateKeys)}]";
        }

        private bool Equals(CompositeKey other)
        {
            var filter = KeyFilterContext.Current;
            return
                ContractKeys.SetEquals(other.ContractKeys)
                && (filter.Filter(typeof(ITagKey)) || TagKeys.SetEquals(other.TagKeys))
                && (filter.Filter(typeof(IStateKey)) || StateKeys.SetEquals(other.StateKeys));
        }

        [CanBeNull]
        private HashSet<T> CreateSet<T>(IEnumerable<T> keys, out int hashCode)
        {
            HashSet<T> set = null;
            hashCode = 1;
            var cnt = 0;
            foreach (var key in keys)
            {
                if (set == null)
                {
                    set = new HashSet<T>();
                }

                set.Add(key);
                cnt++;
                unchecked
                {
                    hashCode += key.GetHashCode();
                }
            }

            unchecked
            {
                hashCode *= cnt;
            }

            return set;
        }
    }
}