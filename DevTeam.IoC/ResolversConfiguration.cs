namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class ResolversConfiguration : IConfiguration
    {
        public static readonly IConfiguration Shared = new ResolversConfiguration();

        private ResolversConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return KeyComparersConfiguration.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            yield return
                resolver
                .Register()
                .Contract(typeof(IResolver<>))
                .Contract(typeof(IProvider<>))
                .KeyComparer(Wellknown.KeyComparers.AnyTagAnyState)
                .AsFactoryMethod(ResolveResolver);

            yield return
                resolver
                .Register()
                .Contract(typeof(IResolver<,>))
                .Contract(typeof(IProvider<,>))
                .KeyComparer(Wellknown.KeyComparers.AnyTagAnyState)
                .AsFactoryMethod(ResolveResolver);

            yield return
                resolver
                .Register()
                .Contract(typeof(IResolver<,,>))
                .Contract(typeof(IProvider<,,>))
                .KeyComparer(Wellknown.KeyComparers.AnyTagAnyState)
                .AsFactoryMethod(ResolveResolver);

            yield return
                resolver
                .Register()
                .Contract(typeof(IResolver<,,,>))
                .Contract(typeof(IProvider<,,,>))
                .KeyComparer(Wellknown.KeyComparers.AnyTagAnyState)
                .AsFactoryMethod(ResolveResolver);

            yield return
                resolver
                .Register()
                .Contract(typeof(IResolver<,,,,>))
                .Contract(typeof(IProvider<,,,,>))
                .KeyComparer(Wellknown.KeyComparers.AnyTagAnyState)
                .AsFactoryMethod(ResolveResolver);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
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