namespace DevTeam.IoC.Contracts
{
    public interface IResolverContext
    {
        IContainer Container { get; }

        IContainer ParentContainer { get; }

        ICompositeKey Key { get; }

        object RegistrationKey { get; }

        IResolverFactory InstanceFactory { get; }

        IStateProvider StateProvider { get; }

        IRegistryContext RegistryContext { get; }
    }
}
