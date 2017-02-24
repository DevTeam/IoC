namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver
    {
        IKeyFactory KeyFactory { [NotNull] get; }

        bool TryCreateResolverContext([NotNull] IKey key, out IResolverContext resolverContext, [CanBeNull] IStateProvider stateProvider = null, [CanBeNull] IContainer container = null);

        [NotNull]
        object Resolve([NotNull] IResolverContext context);
    }
}