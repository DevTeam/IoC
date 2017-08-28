namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [PublicAPI]
    public struct ResolverContext
    {
        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public ResolverContext(
            [NotNull] IContainer container,
            RegistryContext registryContext,
            [NotNull] IInstanceFactory instanceFactory,
            [NotNull] IKey key)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
#endif
            Container = container;
            RegistryContext = registryContext;
            InstanceFactory = instanceFactory;
            Key = key;
        }

        public readonly IContainer Container;

        public readonly IKey Key;

        public readonly IInstanceFactory InstanceFactory;

        public readonly RegistryContext RegistryContext;

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [ Key: {Key}, InstanceFactory: {InstanceFactory}, RegistryContext: {RegistryContext}, Container: {Container}]";
        }
    }
}
