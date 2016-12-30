namespace DevTeam.IoC.Contracts
{
    public interface ICompositeKey: IKey
    {
        IContractKey[] ContractKeys { get; }

        ITagKey[] TagKeys { get; }

        IStateKey[] StateKeys { get; }
    }
}
