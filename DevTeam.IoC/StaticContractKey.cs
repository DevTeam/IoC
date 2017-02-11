namespace DevTeam.IoC
{
    using System.Linq;
    using Contracts;

    internal class StaticContractKey<TContract>
    {
        public static readonly ICompositeKey Shared = new CompositeKey(Enumerable.Repeat<IContractKey>(new ContractKey(typeof(TContract), true), 1), Enumerable.Empty<ITagKey>(), Enumerable.Empty<IStateKey>());
    }
}