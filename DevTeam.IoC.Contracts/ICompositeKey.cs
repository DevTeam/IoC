namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface ICompositeKey: IKey
    {
        ISet<IContractKey> ContractKeys { [NotNull] get; }

        ISet<ITagKey> TagKeys { [NotNull] get; }

        ISet<IStateKey> StateKeys { [NotNull] get; }
    }
}
