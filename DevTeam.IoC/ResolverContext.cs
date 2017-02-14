namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class ResolverContext: IResolverContext
    {
        public ResolverContext(
            [NotNull] IContainer container,
            [NotNull] IRegistryContext registryContext,
            [NotNull] IResolverFactory factory,
            [NotNull] ICompositeKey key,
            [NotNull] object registrationKey,
            [CanBeNull] IStateProvider stateProvider = null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (registrationKey == null) throw new ArgumentNullException(nameof(registrationKey));

            Container = container;
            RegistryContext = registryContext;
            InstanceFactory = factory;
            Key = key;
            RegistrationKey = registrationKey;
            StateProvider = stateProvider ?? EmptyStateProvider.Shared;
        }

        public IContainer Container { get; }

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