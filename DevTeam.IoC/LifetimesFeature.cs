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

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
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
                .Tag(Wellknown.Lifetimes.Singleton)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new SingletonLifetime());

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetimes.AutoDisposing)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new AutoDisposingLifetime());

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetimes.PerResolve)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new KeyBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ResolveId, () => new SingletonLifetime()));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetimes.PerThread)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new KeyBasedLifetime<long>((lifetimeContext, resolverContext) => lifetimeContext.ThreadId, () => new SingletonLifetime()));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetimes.PerContainer)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new KeyBasedLifetime<IResolver>((lifetimeContext, resolverContext) => resolverContext.Container, () => new SingletonLifetime()));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Lifetimes.PerState)
                .Contract<ILifetime>()
                .AsFactoryMethod(ctx => new KeyBasedLifetime<object>((lifetimeContext, resolverContext) => resolverContext.StateProvider.GetKey(resolverContext), () => new SingletonLifetime()));
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