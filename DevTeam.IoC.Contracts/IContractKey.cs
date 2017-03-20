namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IContractKey: IAsymmetricKey
    {
        Type ContractType { [NotNull] get; }

        Type[] GenericTypeArguments { [NotNull] get; }
    }
}
