namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal class ParameterMetadata : IParameterMetadata
    {
        private static readonly IKey[] EmptyKeys = new IKey[0];
        private static readonly object[] EmptyState = new object[0];

        public ParameterMetadata(
            [CanBeNull] IKey[] keys,
            int stateIndex,
            [CanBeNull] object[] state,
            [CanBeNull] object value,
            [CanBeNull] IStateKey stateKey)
        {
#if DEBUG
            if (stateIndex < 0) throw new ArgumentOutOfRangeException(nameof(stateIndex));
#endif
            Keys = keys ?? EmptyKeys;
            State = state ?? EmptyState;
            Value = value;
            StateKey = stateKey;
            IsDependency = keys != null && value == null;
        }

        public bool IsDependency { get; }

        public object[] State { [NotNull] get; }

        public object Value { [CanBeNull] get; }

        public IStateKey StateKey { [CanBeNull] get; }

        public IKey[] Keys { [NotNull] get; }

        protected bool Equals(ParameterMetadata other)
        {
            return IsDependency == other.IsDependency && State.SequenceEqual(other.State) && Equals(Value, other.Value) && Equals(StateKey, other.StateKey) && Keys.SequenceEqual(other.Keys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ParameterMetadata) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}