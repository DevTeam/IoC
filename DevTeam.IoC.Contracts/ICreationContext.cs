namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface ICreationContext
    {
        IResolverContext ResolverContext { [NotNull] get; }

        IStateProvider StateProvider { [NotNull] get; }
    }
}
