namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public static class Extensions
    {
        private static ICompositeKey _fluentKey;

        [NotNull]
        public static IConfiguration Feature([NotNull] this IResolver resolver, Wellknown.Feature feature)
        {
            return resolver.Resolve().Tag(feature).Instance<IConfiguration>();
        }

        [NotNull]
        public static IConfiguring<T> Configure<T>([NotNull] this T container)
            where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return GetFluent(container).Configure(container);
        }

        [NotNull]
        public static IDisposable Register<T>([NotNull] this T registry, [NotNull] IRegistryContext context)
            where T : IRegistry
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            IDisposable registration;
            if (!registry.TryRegister(context, out registration))
            {
                throw new InvalidOperationException($"Can not register {string.Join(Environment.NewLine, context.Keys)}.{Environment.NewLine}{Environment.NewLine}{registry}");
            }

            return registration;
        }

        [NotNull]
        public static IRegistration<T> Register<T>([NotNull] this T container)
              where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return GetFluent(container).Register(container);
        }

        [NotNull]
        public static IContainer CreateChild<T>([NotNull] this T container, [CanBeNull] object tag = null)
             where T : IResolver, IRegistry
        {
            if (tag == null)
            {
                return container.Resolve().Instance<IContainer>();
            }

            return container.Resolve().State<object>(0).Instance<IContainer>(tag);
        }

        [NotNull]
        public static IResolving<T> Resolve<T>([NotNull] this T resolver)
              where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return GetFluent(resolver).Resolve(resolver);
        }

        [CanBeNull]
        public static object TryGetState([NotNull] this IResolverContext ctx, int index, [NotNull] Type stateType)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return ctx.StateProvider.GetState(ctx, ctx.Container.KeyFactory.CreateStateKey(index, stateType));
        }

        [NotNull]
        public static object GetState([NotNull] this IResolverContext ctx, int index, [NotNull] Type stateType)
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

        [CanBeNull]
        public static T TryGetState<T>([NotNull] this IResolverContext ctx, int index)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)ctx.TryGetState(index, typeof(T));
        }

        [NotNull]
        public static T GetState<T>([NotNull] this IResolverContext ctx, int index)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return (T)GetState(ctx, index, typeof(T));
        }

        [NotNull]
        private static IFluent GetFluent<T>([NotNull] T resolver)
             where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (_fluentKey == null)
            {
                _fluentKey = resolver.KeyFactory.CreateCompositeKey(new[] { resolver.KeyFactory.CreateContractKey(typeof(IFluent), true) });
            }

            IResolverContext ctx;
            resolver.TryCreateContext(_fluentKey, out ctx);
            return (IFluent)resolver.Resolve(ctx);
        }
    }
}