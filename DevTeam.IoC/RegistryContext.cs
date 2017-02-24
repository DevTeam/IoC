namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class RegistryContext : IRegistryContext
    {
        public RegistryContext(
            [NotNull] IContainer container,
            [NotNull] IKey[] keys,
            [NotNull] IResolverFactory factory,
            [NotNull] IEnumerable<IExtension> extensionPoints)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensionPoints == null) throw new ArgumentNullException(nameof(extensionPoints));

            Container = container;
            Keys = keys;
            InstanceFactory = factory;
            Extensions = extensionPoints.ToArray();
        }

        public IContainer Container { get; }

        public IEnumerable<IKey> Keys { get; }

        public IResolverFactory InstanceFactory { get; }

        public IEnumerable<IExtension> Extensions { get; }

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [Container: {Container.ToString() ?? "null"}, Keys: {string.Join(", ", Keys)}, InstanceFactory: {InstanceFactory} , Extensions: {string.Join(", ", Extensions)}]";
        }
    }
}