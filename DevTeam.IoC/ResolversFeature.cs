namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal sealed class ResolversFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new ResolversFeature();

        private ResolversFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return KeyComparersFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<>))
                .Contract(typeof(IProvider<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveResolver(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(Func<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveFunc(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,>))
                .Contract(typeof(IProvider<,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveResolver(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(Func<,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveFunc(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,>))
                .Contract(typeof(IProvider<,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveResolver(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveFunc(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,,>))
                .Contract(typeof(IProvider<,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveResolver(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveFunc(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(IResolver<,,,,>))
                .Contract(typeof(IProvider<,,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveResolver(ctx, reflection));

            yield return
                container
                .Register()
                .Contract(typeof(Func<,,,,>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveFunc(ctx, reflection));
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static object ResolveFunc(ICreationContext ctx, IReflection reflection)
        {
            return ((IFuncProvider)ResolveResolver(ctx, reflection)).GetFunc();
        }

        private static object ResolveResolver(ICreationContext creationContext, IReflection reflection)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
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

            var ctor = reflection.GetType(resolverType).Constructors.Single(i => i.GetParameters().Length == 1);
            var factory = ctx.Container.Resolve().Instance<IMethodFactory>(creationContext.StateProvider);
            return factory.CreateConstructor(ctor)(ctx);
        }
    }
}