namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface ICompositeKey: IKey
    {
        HashSet<IContractKey> ContractKeys { [NotNull] get; }

        HashSet<ITagKey> TagKeys { [NotNull] get; }

        HashSet<IStateKey> StateKeys { [NotNull] get; }
    }
}
