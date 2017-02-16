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
            [NotNull] ICompositeKey key,
            [NotNull] object registrationKey,
            [CanBeNull] IStateProvider stateProvider = null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (registrationKey == null) throw new ArgumentNullException(nameof(registrationKey));
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            Container = container;
            RegistryContext = registryContext;
            InstanceFactory = instanceFactory;
            Key = key;
            RegistrationKey = registrationKey;
            StateProvider = stateProvider ?? EmptyStateProvider.Shared;
        }

        public IContainer Container { get; set; }

        public ICompositeKey Key { get; }

        public object RegistrationKey { get; }

        public IResolverFactory InstanceFactory { get; }

        public IStateProvider StateProvider { get; }

        public IRegistryContext RegistryContext { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [Container: {Container}, Key: {Key}, RegistrationKey: {RegistrationKey}, InstanceFactory: {InstanceFactory}, StateProvider: {StateProvider} , RegistryContext: {RegistryContext}]";
        }
    }
}