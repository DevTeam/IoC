namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class ContainerConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new ContainerConfiguration();
        private static readonly IEnumerable<IKey> InternalResourceStoreKeys = LowLevelRegistration.CreateKeys<IInternalResourceStore>(RootContainerConfiguration.Reflection);
        private static readonly InternalScope InternalScope = new InternalScope();

        private ContainerConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var internalResourceStore = new InternalResourceStore();
            yield return LowLevelRegistration.RawRegister(container, InternalResourceStoreKeys, ctx => internalResourceStore, InternalScope);
            yield return internalResourceStore;
        }
    }
}
