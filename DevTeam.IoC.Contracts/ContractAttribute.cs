namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ContractAttribute : Attribute
    {
        public ContractAttribute(params Type[] contractTypes)
        {
            if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
            ContractTypes = contractTypes;
        }

        public Type[] ContractTypes { get; }
    }
}