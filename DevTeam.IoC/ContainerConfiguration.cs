namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class ContainerConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new ContainerConfiguration();
        private static readonly IEnumerable<ICompositeKey> InternalResourceStoreKeys = LowLevelRegistration.CreateKeys<IInternalResourceStore>();

        private ContainerConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var container = resolver as Container;
            if (container == null) throw new ArgumentException(nameof(resolver));
            var internalResourceStore = new InternalResourceStore();
            yield return LowLevelRegistration.RawRegister(container, InternalResourceStoreKeys, ctx => internalResourceStore);
            yield return internalResourceStore;
        }
    }
}
