namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class LifetimesFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new LifetimesFeature();

        private LifetimesFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return 
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.Singleton)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonLifetime());

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.AutoDisposing)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new AutoDisposingLifetime());

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.PerResolve)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ResolveId));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.PerThread)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ThreadId));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.PerContainer)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonBasedLifetime<IResolver>((lifetimeContext, resolverContext) => resolverContext.Container));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetime.PerState)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonBasedLifetime<object>((lifetimeContext, resolverContext) => resolverContext.StateProvider.GetKey(resolverContext)));
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }
    }
}