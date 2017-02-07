namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver
    {
        IKeyFactory KeyFactory { get; }

        bool TryCreateContext(ICompositeKey key, out IResolverContext resolverContext, IStateProvider stateProvider = null);

        object Resolve(IResolverContext context);
    }
}