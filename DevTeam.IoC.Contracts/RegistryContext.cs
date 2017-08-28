namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    [PublicAPI]
    public struct RegistryContext
    {
        private static long _currentId;
        
        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public RegistryContext(
            [NotNull] IContainer container,
            [NotNull] IKey[] keys,
            [NotNull] IInstanceFactory factory,
            [NotNull] params IExtension[] extensions)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
#endif
            Id = Interlocked.Increment(ref _currentId);
            Container = container;
            Keys = keys;
            InstanceFactory = factory;
            Extensions = extensions;
        }

        public readonly long Id;

        public readonly IContainer Container;

        public readonly IEnumerable<IKey> Keys;

        public readonly IInstanceFactory InstanceFactory;

        public readonly IEnumerable<IExtension> Extensions;

        public override string ToString()
        {
            return $"{nameof(RegistryContext)} [Keys: {string.Join(", ", Keys.Select(i => i.ToString()).ToArray())}, InstanceFactory: {InstanceFactory}, Extensions: {string.Join(", ", Extensions.Select(i => i.ToString()).ToArray())}, Container: {Container}]";
        }

    }
}
