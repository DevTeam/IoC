namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolverContext
    {
        IContainer Container { [NotNull] get; }

        IKey Key { [NotNull] get; }

        IResolverFactory InstanceFactory { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
