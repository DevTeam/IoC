namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class ResolversFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new ResolversFeature();

        private ResolversFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return KeyComparersFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return
                container
                .Register()
                .Contract(typeof(IResolver<>))
                .Contract(typeof(IProvider<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveResolver);

            yield return
                container
                .Register()
                .Contract(typeof(Func<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveFunc);

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,>))
                .Contract(typeof(IProvider<,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveResolver);

            yield return
                container
                .Register()
                .Contract(typeof(Func<,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveFunc);

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,>))
                .Contract(typeof(IProvider<,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveResolver);

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveFunc);

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,,>))
                .Contract(typeof(IProvider<,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveResolver);

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveFunc);

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,,,>))
                .Contract(typeof(IProvider<,,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveResolver);

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveFunc);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static object ResolveFunc(IResolverContext ctx)
        {
            return ((IFuncProvider)ResolveResolver(ctx)).GetFunc();
        }

        private static object ResolveResolver(IResolverContext ctx)
        {
            var genericContractKey = ctx.Key.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var genericTypeArguments = genericContractKey.GenericTypeArguments.ToArray();
            Type resolverType;
            switch (genericTypeArguments.Length)
            {
                case 2:
                    resolverType = typeof(Resolver<,>).MakeGenericType(genericTypeArguments);
                    break;

                case 3:
                    resolverType = typeof(Resolver<,,>).MakeGenericType(genericTypeArguments);
                    break;

                case 4:
                    resolverType = typeof(Resolver<,,,>).MakeGenericType(genericTypeArguments);
                    break;

                case 5:
                    resolverType = typeof(Resolver<,,,,>).MakeGenericType(genericTypeArguments);
                    break;

                default:
                    resolverType = typeof(Resolver<>).MakeGenericType(genericContractKey.GenericTypeArguments.ToArray());
                    break;
            }

            var ctor = resolverType.GetTypeInfo().DeclaredConstructors.Single(i => i.GetParameters().Length == 1);
            var factory = ctx.Container.Resolve().Instance<IInstanceFactoryProvider>(ctx.StateProvider);
            return factory.GetFactory(ctor).Create(ctx);
        }
    }
}