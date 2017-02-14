namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class ContainerConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new ContainerConfiguration();
        private static readonly IEnumerable<ICompositeKey> InternalResourceStoreKeys = LowLevelRegistration.CreateKeys<IInternalResourceStore>();
        private static readonly PrivateScope PrivateScope = new PrivateScope();

        private ContainerConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var internalResourceStore = new InternalResourceStore();
            yield return LowLevelRegistration.RawRegister(container, InternalResourceStoreKeys, ctx => internalResourceStore, PrivateScope);
            yield return internalResourceStore;
        }
    }
}
