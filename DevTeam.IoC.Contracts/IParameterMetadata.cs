namespace DevTeam.IoC.Contracts
{
    public interface IParameterMetadata
    {
        bool IsDependency { get; }

        IKey[] Keys { get; }

        object[] State { get; }

        object Value { get; }

        IStateKey StateKey { get; }
    }
}