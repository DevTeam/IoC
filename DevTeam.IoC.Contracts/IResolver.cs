namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver
    {
        IKeyFactory KeyFactory { [NotNull] get; }

        bool TryCreateResolverContext([NotNull] IKey key, out ResolverContext resolverContext, [CanBeNull] IContainer container = null);

        [NotNull]
        object Resolve(ResolverContext context, [CanBeNull] IStateProvider stateProvider = null);
    }
}