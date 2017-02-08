namespace DevTeam.IoC
{
    using System.Linq;
    using Contracts;

    internal class StaticContractKey<TContract>
    {
        public static readonly ICompositeKey Shared = new CompositeKey(new IContractKey[] { new ContractKey(typeof(TContract), true) }, Enumerable.Empty<ITagKey>(), Enumerable.Empty<IStateKey>());
    }
}