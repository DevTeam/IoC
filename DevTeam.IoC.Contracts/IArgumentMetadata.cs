namespace DevTeam.IoC.Contracts
{
    public interface IArgumentMetadata
    {
        bool IsDependency { get; }

        IKey[] Keys { get; }

        object[] State { get; }

        IStateKey StateKey { get; }
    }
}