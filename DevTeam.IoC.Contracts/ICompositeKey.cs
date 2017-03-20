namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface ICompositeKey: IKey
    {
        IEnumerable<IContractKey> ContractKeys { [NotNull] get; }

        IEnumerable<ITagKey> TagKeys { [NotNull] get; }

        IEnumerable<IStateKey> StateKeys { [NotNull] get; }
    }
}
