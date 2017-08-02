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
                    .Contract(typeof(Lazy<>))
                    .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                    .FactoryMethod(ctx => ResolveLazy(ctx, reflection));

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

        private static object ResolveLazy(ICreationContext creationContext, IReflection reflection)
        {
            var resolverContext = creationContext.ResolverContext;
            var genericTypeArguments = GetGenericTypeArguments(creationContext);
            if (genericTypeArguments.Length != 1)
            {
                throw new ContainerException($"Can not define a generic type argument for Lazy<>.\nDetails:\n{creationContext}");
            }

            var lazyType = typeof(Lazy<>).MakeGenericType(genericTypeArguments);
            var factory = resolverContext.Container.Resolve().Instance<IMethodFactory>(creationContext.StateProvider);

#if NET35
            var ctors = 
                from ctor in reflection.GetType(lazyType).Constructors
                let parameters = ctor.GetParameters()
                where parameters.Length == 1
                select ctor;
            return factory.CreateConstructor(ctors.Single())(ResolveFunc(creationContext, reflection));
#else
            var ctors = 
                from ctor in reflection.GetType(lazyType).Constructors
                let parameters = ctor.GetParameters()
                where parameters.Length == 2 && parameters[1].ParameterType == typeof(bool)
                select ctor;
            return factory.CreateConstructor(ctors.Single())(ResolveFunc(creationContext, reflection), true /*thread safe Lazy<>*/);
#endif
        }

        private static object ResolveResolver(ICreationContext creationContext, IReflection reflection)
        {
            var resolverContext = creationContext.ResolverContext;
            var genericTypeArguments = GetGenericTypeArguments(creationContext);
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
                    resolverType = typeof(Resolver<>).MakeGenericType(genericTypeArguments);
                    break;
            }

            var ctor = reflection.GetType(resolverType).Constructors.Single(i => i.GetParameters().Length == 1);
            var factory = resolverContext.Container.Resolve().Instance<IMethodFactory>(creationContext.StateProvider);
            return factory.CreateConstructor(ctor)(resolverContext);
        }

        private static Type[] GetGenericTypeArguments(ICreationContext creationContext)
        {
            var genericContractKey = creationContext.ResolverContext.Key as IContractKey ?? (creationContext.ResolverContext.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new ContainerException($"Can not define a generic type arguments for Lazy<>.\nDetails:\n{creationContext}");
            }

            var genericTypeArguments = genericContractKey.GenericTypeArguments.ToArray();
            return genericTypeArguments;
        }
    }
}