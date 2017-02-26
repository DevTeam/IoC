namespace DevTeam.IoC
{
    using Contracts;

    internal class StaticContractKey<TContract>
    {
        public static readonly IKey Shared = new ContractKey(typeof(TContract), true);
    }
}