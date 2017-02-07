namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IContractKey: IKey
    {
        Type ContractType { [NotNull] get; }

        Type[] GenericTypeArguments { [NotNull] get; }

        bool ToResolve { get; }
    }
}
