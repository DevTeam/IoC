namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface ICompositeKey: IKey
    {
        IKeySet<IContractKey> ContractKeys { [NotNull] get; }

        IKeySet<ITagKey> TagKeys { [NotNull] get; }

        IKeySet<IStateKey> StateKeys { [NotNull] get; }
    }
}
