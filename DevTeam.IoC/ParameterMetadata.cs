namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal sealed class ParameterMetadata : IParameterMetadata
    {
        private static readonly object[] EmptyState = new object[0];

        public ParameterMetadata(
            [CanBeNull] IContractKey[] contractKeys,
            [CanBeNull] ITagKey[] tagKeys,
            [CanBeNull] IStateKey[] stateKeys,
            int stateIndex,
            [CanBeNull] object[] state,
            [CanBeNull] object value,
            [CanBeNull] IStateKey stateKey)
        {
#if DEBUG
            if (stateIndex < 0) throw new ArgumentOutOfRangeException(nameof(stateIndex));
#endif
            State = state ?? EmptyState;
            ContractKeys = contractKeys;
            TagKeys = tagKeys;
            StateKeys = stateKeys;
            Value = value;
            StateKey = stateKey;
            IsDependency = StateKey == null && value == null;
        }

        public bool IsDependency { get; }

        public object[] State { [NotNull] get; }

        public IContractKey[] ContractKeys { get; }

        public ITagKey[] TagKeys { get; }

        public IStateKey[] StateKeys { get; }

        public object Value { [CanBeNull] get; }

        public IStateKey StateKey { [CanBeNull] get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as IParameterMetadata;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return 0;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IParameterMetadata other)
        {
            return IsDependency == other.IsDependency && Equals(ContractKeys, other.ContractKeys) && Equals(TagKeys, other.TagKeys) && Equals(StateKeys, other.StateKeys) && Equals(Value, other.Value) && Equals(StateKey, other.StateKey) && Equals(State, other.State);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals<T>([CanBeNull] IEnumerable<T> s1, [CanBeNull] IEnumerable<T> s2)
        {
            if (object.Equals(s1, s2))
            {
                return true;
            }

            if (s1 == null || s2 == null)
            {
                return false;
            }

            return s1.SequenceEqual(s2);
        }
    }
}