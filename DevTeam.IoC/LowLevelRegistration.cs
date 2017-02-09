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
            var key = new CompositeKey(new[] { (IContractKey)new ContractKey(typeof(TContract), true), }, Enumerable.Empty<ITagKey>(), Enumerable.Empty<IStateKey>());
            return Enumerable.Repeat(key, 1).ToArray();
        }

        public static IDisposable RawRegister<TContract>(IRegistry registry, IEnumerable<ICompositeKey> keys, Func<IResolverContext, TContract> factoryMethod)
        {
            var registryContext = registry.CreateContext(keys, new MethodFactory<TContract>(factoryMethod), Enumerable.Empty<IExtension>());
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }
    }
}
