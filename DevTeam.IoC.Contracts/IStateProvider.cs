namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IStateProvider
    {
        [NotNull]
        object GetKey([NotNull] IResolverContext resolverContext);

        [CanBeNull]
        object GetState([NotNull] IResolverContext resolverContext, [NotNull] IStateKey stateKey);
    }
}
