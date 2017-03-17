namespace DevTeam.IoC
{
    using Contracts;

    internal static class StaticContractKey<TContract>
    {
        public static readonly IKey Shared = new ContractKey(RootContainerConfiguration.Reflection, typeof(TContract), true);
    }
}