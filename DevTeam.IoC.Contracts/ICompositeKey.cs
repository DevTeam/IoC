namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface ICompositeKey: IKey
    {
        IContractKey[] ContractKeys { [NotNull] get; }

        ITagKey[] TagKeys { [NotNull] get; }

        IStateKey[] StateKeys { [NotNull] get; }
    }
}
