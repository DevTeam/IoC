namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal static class LowLevelRegistration
    {
        public static IEnumerable<IKey> CreateKeys<TContract>()
        {
            return CreateKeys(typeof(TContract));
        }

        public static IEnumerable<IKey> CreateKeys(Type contractType)
        {
            return Enumerable.Repeat((IKey)new ContractKey(contractType, true), 1).ToArray();
        }

        public static IDisposable RawRegister<TContract>(IRegistry registry, IEnumerable<IKey> keys, Func<ICreationContext, TContract> factoryMethod, params IExtension[] extensions)
        {
            var registryContext = registry.CreateRegistryContext(keys, new MethodFactory<TContract>(factoryMethod), extensions);
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }

        public static IDisposable RawRegister(Type contractType, IRegistry registry, IEnumerable<IKey> keys, Func<ICreationContext, object> factoryMethod, params IExtension[] extensions)
        {
            var registryContext = registry.CreateRegistryContext(keys, new MethodFactory<object>(factoryMethod), extensions);
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }
    }
}
