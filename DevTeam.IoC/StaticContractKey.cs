namespace DevTeam.IoC
{
    using Contracts;

    internal static class StaticContractKey<TContract>
    {
        public static readonly IKey Shared = new ContractKey(typeof(TContract), true);
    }
}