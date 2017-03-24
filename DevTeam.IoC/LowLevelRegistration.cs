namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal static class LowLevelRegistration
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<IKey> CreateKeys<TContract>()
        {
            return CreateKeys(Reflection.Shared, typeof(TContract));
        }

        public static IDisposable RawRegister<TContract>(
            [NotNull] IRegistry registry,
            [NotNull] IEnumerable<IKey> keys,
            [NotNull] Func<ICreationContext, TContract> factoryMethod,
            [NotNull] params IExtension[] extensions)
        {
#if DEBUG
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
#endif
            var registryContext = registry.CreateRegistryContext(keys, new MethodFactory<TContract>(factoryMethod), extensions);
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }

        public static IDisposable RawRegister(
            [NotNull] Type contractType,
            [NotNull] IRegistry registry,
            [NotNull] IEnumerable<IKey> keys,
            [NotNull] Func<ICreationContext, object> factoryMethod,
            [NotNull] params IExtension[] extensions)
        {
#if DEBUG
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
#endif
            var registryContext = registry.CreateRegistryContext(keys, new MethodFactory<object>(factoryMethod), extensions);
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static IEnumerable<IKey> CreateKeys([NotNull] IReflection reflection, [NotNull] Type contractType)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
#endif
            return Enumerable.Repeat((IKey)new ContractKey(reflection, contractType, true), 1).ToArray();
        }
    }
}
