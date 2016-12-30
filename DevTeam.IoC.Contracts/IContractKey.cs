namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IContractKey: IKey
    {
        Type ContractType { get; }

        Type[] GenericTypeArguments { get; }

        bool ToResolve { get; }
    }
}
