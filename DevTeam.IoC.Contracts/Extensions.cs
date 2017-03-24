namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public static class Extensions
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
        public static IConfiguring<T> Configure<T>([NotNull] this T container)
            where T : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return Fluent(container).Configure(container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IRegistration<T> Register<T>([NotNull] this T container)
              where T : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return Fluent(container).Register(container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IResolving<T> Resolve<T>([NotNull] this T resolver)
              where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return Fluent(resolver).Resolve(resolver);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IContainer CreateChild<T>([NotNull] this T container, [CanBeNull] object tag = null)
             where T : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (tag == null)
            {
                return container.Resolve().Instance<IContainer>();
            }

            return container.Resolve().State<object>(0).Instance<IContainer>(tag);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CanBeNull]
        public static object TryGetState([NotNull] this ICreationContext ctx, int index, [NotNull] Type stateType)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return ctx.StateProvider.GetState(ctx, ctx.ResolverContext.Container.KeyFactory.CreateStateKey(index, stateType, true));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static object GetState([NotNull] this ICreationContext ctx, int index, [NotNull] Type stateType)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            var value = TryGetState(ctx, index, stateType);
            if (value == null)
            {
                throw new InvalidOperationException($"State {index} of type \"{stateType.FullName}\" can not be null.");
            }

            return value;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [CanBeNull]
        public static T TryGetState<T>([NotNull] this ICreationContext ctx, int index)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)ctx.TryGetState(index, typeof(T));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static T GetState<T>([NotNull] this ICreationContext ctx, int index)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)GetState(ctx, index, typeof(T));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        [NotNull]
        public static IFluent Fluent<T>([NotNull] this T resolver)
             where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var fluentProvider = resolver as IProvider<IFluent>;
            if (fluentProvider == null || !fluentProvider.TryGet(out IFluent fluent))
            {
                throw new InvalidOperationException($"{typeof(IProvider<IFluent>)} is not supported. Only \"{nameof(IContainer)}\" is supported.");
            }

            return fluent;
        }
    }
}