namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IParameterMetadata
    {
        Type Type { [NotNull] get; }

        bool IsDependency { get; }

        IKey[] Keys { [CanBeNull] get; }

        object[] State { [CanBeNull] get; }

        object Value { [CanBeNull] get; }

        IStateKey StateKey { [CanBeNull] get; }
    }
}