namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
    public class ContractAttribute : Attribute
    {
        public ContractAttribute([NotNull][ItemNotNull] params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            ContractTypes = contractTypes;
        }

        public Type[] ContractTypes { [NotNull] get; }
    }
}