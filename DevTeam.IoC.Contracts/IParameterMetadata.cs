namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IParameterMetadata
    {
        bool IsDependency { get; }

        IContractKey[] ContractKeys { [CanBeNull] get; }

        ITagKey[] TagKeys { [CanBeNull] get; }

        IStateKey[] StateKeys { [CanBeNull] get; }

        object[] State { [CanBeNull] get; }

        object Value { [CanBeNull] get; }

        IStateKey StateKey { [CanBeNull] get; }
    }
}