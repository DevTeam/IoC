﻿namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public static class Containers
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IConfiguration Feature([NotNull] this IResolver resolver, [NotNull] object feature)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            return resolver.Resolve().Tag(feature).Instance<IConfiguration>();
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IConfiguring<TContainer> Configure<TContainer>([NotNull] this TContainer container)
            where TContainer : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return GetFluent(container).Configure(container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IRegistration<TContainer> Register<TContainer>([NotNull] this TContainer container)
              where TContainer : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return GetFluent(container).Register(container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IResolving<TResolver> Resolve<TResolver>([NotNull] this TResolver resolver)
              where TResolver : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return GetFluent(resolver).Resolve(resolver);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IContainer CreateChild<TResolver>([NotNull] this TResolver resolver, [CanBeNull] object tag = null)
             where TResolver : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (tag == null)
            {
                return resolver.Resolve().Instance<IContainer>();
            }

            return resolver.Resolve().State<object>(0).Instance<IContainer>(tag);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static T GetState<T>(this CreationContext ctx, int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)GetState(ctx, index, typeof(T));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CanBeNull]
        public static T TryGetState<T>(this CreationContext ctx, int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)ctx.TryGetState(index, typeof(T));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CanBeNull]
        private static object TryGetState(this CreationContext ctx, int index, [NotNull] Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return ctx.StateProvider.GetState(ctx, ctx.ResolverContext.Container.KeyFactory.CreateStateKey(index, stateType, true));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        private static object GetState(this CreationContext ctx, int index, [NotNull] Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            var value = TryGetState(ctx, index, stateType);
            if (value == null)
            {
                throw new ContainerException($"State {index} of type \"{stateType.FullName}\" can not be null.");
            }

            return value;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        private static IFluent GetFluent<TResolver>([NotNull] this TResolver resolver)
             where TResolver : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (resolver is IProvider<IFluent> fluentProvider && fluentProvider.TryGet(out var fluent))
            {
                return fluent;
            }

            throw new ContainerException($"{typeof(IProvider<IFluent>)} is not supported.\nDetails:\n{resolver}");
        }
    }
}