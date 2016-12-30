namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class RegistryContext : IRegistryContext
    {
        public RegistryContext(
            IContainer container,
            IContainer parentContainer,
            ICompositeKey[] keys,
            IResolverFactory factory,
            IEnumerable<IExtension> extensionPoints)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensionPoints == null) throw new ArgumentNullException(nameof(extensionPoints));

            Container = container;
            ParentContainer = parentContainer;
            Keys = keys;
            InstanceFactory = factory;
            Extensions = extensionPoints.ToArray();
        }

        public IContainer Container { get; }

        public IContainer ParentContainer { get; }

        public IEnumerable<ICompositeKey> Keys { get; }

        public IResolverFactory InstanceFactory { get; }

        public IEnumerable<IExtension> Extensions { get; }
    }
}