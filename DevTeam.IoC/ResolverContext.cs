namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class ResolverContext: IResolverContext
    {
        public ResolverContext(
            [NotNull] IContainer container,
            [NotNull] IRegistryContext registryContext,
            [NotNull] IResolverFactory instanceFactory,
            [NotNull] IKey key,
            [CanBeNull] IStateProvider stateProvider = null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            Container = container;
            RegistryContext = registryContext;
            InstanceFactory = instanceFactory;
            Key = key;
            StateProvider = stateProvider ?? EmptyStateProvider.Shared;
        }

        public IContainer Container { get; set; }

        public IKey Key { get; }

        public IResolverFactory InstanceFactory { get; }

        public IStateProvider StateProvider { get; }

        public IRegistryContext RegistryContext { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [ Key: {Key}, InstanceFactory: {InstanceFactory}, StateProvider: {StateProvider} , RegistryContext: {RegistryContext}, Container: {Container}]";
        }
    }
}