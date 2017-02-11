namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal static class LowLevelRegistration
    {
        public static IEnumerable<ICompositeKey> CreateKeys<TContract>()
        {
            return CreateKeys(typeof(TContract));
        }

        public static IEnumerable<ICompositeKey> CreateKeys(Type contractType)
        {
            var key = RootConfiguration.KeyFactory.CreateCompositeKey(Enumerable.Repeat<IContractKey>(new ContractKey(contractType, true), 1));
            return Enumerable.Repeat(key, 1).ToArray();
        }

        public static IDisposable RawRegister<TContract>(IRegistry registry, IEnumerable<ICompositeKey> keys, Func<IResolverContext, TContract> factoryMethod)
        {
            var registryContext = registry.CreateContext(keys, new MethodFactory<TContract>(factoryMethod), Enumerable.Empty<IExtension>());
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }

        public static IDisposable RawRegister(Type contractType, IRegistry registry, IEnumerable<ICompositeKey> keys, Func<IResolverContext, object> factoryMethod)
        {
            var registryContext = registry.CreateContext(keys, new MethodFactory<object>(factoryMethod), Enumerable.Empty<IExtension>());
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }
    }
}
