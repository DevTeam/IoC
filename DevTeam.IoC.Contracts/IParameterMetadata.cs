namespace DevTeam.IoC.Contracts
{
    public interface IParameterMetadata
    {
        bool IsDependency { get; }

        IKey[] Keys { get; }

        object[] State { get; }

        IStateKey StateKey { get; }
    }
}