namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal struct RegistryContext : IRegistryContext
    {
        public RegistryContext(
            [NotNull] IContainer container,
            [NotNull] IKey[] keys,
            [NotNull] IResolverFactory factory,
            [NotNull] params IExtension[] extensions)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
#endif
            Container = container;
            Keys = keys;
            InstanceFactory = factory;
            Extensions = extensions;
        }

        public IContainer Container { get; }

        public IEnumerable<IKey> Keys { get; }

        public IResolverFactory InstanceFactory { get; }

        public IEnumerable<IExtension> Extensions { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [Keys: {string.Join(", ", Keys)}, InstanceFactory: {InstanceFactory}, Extensions: {string.Join(", ", Extensions)}, Container: {Container.ToString() ?? "null"}]";
        }
    }
}