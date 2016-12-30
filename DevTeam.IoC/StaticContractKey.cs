namespace DevTeam.IoC
{
    using Contracts;

    internal class StaticContractKey<TContract>
    {
        public static readonly ICompositeKey Shared = new CompositeKey(new IContractKey[] { new ContractKey(typeof(TContract), true) }, new ITagKey[0], new IStateKey[0]);
    }
}