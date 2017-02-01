namespace DevTeam.IoC.Contracts
{
    using System;

    public static class ContainerExtensions
    {
        private static ICompositeKey _fluentKey;

        public static IConfiguration Feature(this IResolver resolver, Wellknown.Features feature)
        {
            return resolver.Resolve().Tag(feature).Instance<IConfiguration>();
        }

        public static IConfiguring Configure(this IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return GetFluent(resolver).Configure(resolver);
        }

        public static IDisposable Register(this IResolver resolver, IRegistryContext context)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (context == null) throw new ArgumentNullException(nameof(context));
            IRegistry registry;
            IDisposable registration;
            if (!GetFluent(resolver).TryGetRegistry(out registry) || !registry.TryRegister(context, out registration))
            {
                throw new InvalidOperationException($"Can't register {string.Join(Environment.NewLine, context.Keys)}.{Environment.NewLine}{Environment.NewLine}{registry}");
            }

            return registration;
        }

        public static IRegistration Register(this IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return GetFluent(resolver).Register();
        }

        public static IContainer CreateChild(this IResolver resolver, object tag = null)
        {
            if (tag == null)
            {
                return resolver.Resolve().Instance<IContainer>();
            }

            return resolver.Resolve().State<object>(0).Instance<IContainer>("child");
        }

        public static IResolving Resolve(this IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return GetFluent(resolver).Resolve();
        }

        public static object GetState(this IResolverContext ctx, int index, Type stateType)
        {
            return ctx.StateProvider.GetState(ctx, ctx.Container.KeyFactory.CreateStateKey(index, stateType));
        }

        public static T GetState<T>(this IResolverContext ctx, int index)
        {
            return (T)ctx.GetState(index, typeof(T));
        }

        private static IFluent GetFluent(IResolver resolver)
        {
            if (_fluentKey == null)
            {
                _fluentKey = resolver.KeyFactory.CreateCompositeKey(new[] { resolver.KeyFactory.CreateContractKey(typeof(IFluent), true) }, new ITagKey[0], new IStateKey[0]);
            }
            IResolverContext ctx;
            resolver.TryCreateContext(_fluentKey, out ctx);
            return (IFluent)resolver.Resolve(ctx);
        }
    }
}