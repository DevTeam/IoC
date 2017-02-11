namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IParameterMetadata
    {
        bool IsDependency { get; }

        IKey[] Keys { [CanBeNull] get; }

        object[] State { [CanBeNull] get; }

        object Value { [CanBeNull] get; }

        IStateKey StateKey { [CanBeNull] get; }
    }
}