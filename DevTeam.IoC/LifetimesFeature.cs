﻿namespace DevTeam.IoC
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

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return 
                container
                .Register()
                .Tag(Wellknown.Lifetime.Singleton)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new SingletonLifetime())
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Lifetime.AutoDisposing)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new AutoDisposingLifetime())
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Lifetime.PerResolve)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new SingletonBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ResolveId))
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Lifetime.PerThread)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new SingletonBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ThreadId))
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Lifetime.PerContainer)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new SingletonBasedLifetime<IResolver>((lifetimeContext, resolverContext) => resolverContext.Container))
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Lifetime.PerState)
                .Contract<ILifetime>()
                .FactoryMethod(ctx => new SingletonBasedLifetime<object>((lifetimeContext, resolverContext) => resolverContext.StateProvider.GetKey(resolverContext)))
                .Apply();
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