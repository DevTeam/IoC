namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver
    {
        IKeyFactory KeyFactory { [NotNull] get; }

        bool TryCreateResolverContext([NotNull] ICompositeKey key, out IResolverContext resolverContext, IStateProvider stateProvider = null);

        [NotNull]
        object Resolve([NotNull] IResolverContext context);
    }
}