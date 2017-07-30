namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
    public class ContractAttribute : Attribute
    {
        public ContractAttribute([NotNull][ItemNotNull] params Type[] contractTypes)
        {
            ContractTypes = contractTypes ?? throw new ArgumentNullException(nameof(contractTypes));
        }

        public Type[] ContractTypes { [NotNull] get; }
    }
}