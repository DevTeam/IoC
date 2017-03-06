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
            [NotNull] IKey key)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
#endif
            Container = container;
            RegistryContext = registryContext;
            InstanceFactory = instanceFactory;
            Key = key;
        }

        public IContainer Container { get; set; }

        public IKey Key { get; }

        public IResolverFactory InstanceFactory { get; }

        public IRegistryContext RegistryContext { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [ Key: {Key}, InstanceFactory: {InstanceFactory}, RegistryContext: {RegistryContext}, Container: {Container}]";
        }
    }
}