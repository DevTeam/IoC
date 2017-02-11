namespace DevTeam.IoC
{
    using System.Linq;
    using Contracts;

    internal class StaticContractKey<TContract>
    {
        public static readonly ICompositeKey Shared = RootConfiguration.KeyFactory.CreateCompositeKey(Enumerable.Repeat<IContractKey>(new ContractKey(typeof(TContract), true), 1));
    }
}