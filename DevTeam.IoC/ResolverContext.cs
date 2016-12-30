namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class ResolverContext: IResolverContext
    {
        public ResolverContext(
            IContainer container,
            IContainer parentContainer,
            IRegistryContext registryContext,
            IResolverFactory factory,
            ICompositeKey key,
            object registrationKey,
            IStateProvider stateProvider = null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (registrationKey == null) throw new ArgumentNullException(nameof(registrationKey));

            Container = container;
            ParentContainer = parentContainer;
            RegistryContext = registryContext;
            InstanceFactory = factory;
            Key = key;
            RegistrationKey = registrationKey;
            StateProvider = stateProvider ?? EpmtyStateProvider.Shared;
        }

        public IContainer Container { get; }

        public IContainer ParentContainer { get; }

        public ICompositeKey Key { get; }

        public object RegistrationKey { get; }

        public IResolverFactory InstanceFactory { get; }

        public IStateProvider StateProvider { get; }

        public IRegistryContext RegistryContext { get; }
    }
}