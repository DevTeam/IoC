﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class CacheFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new CacheFeature();

        private CacheFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield return
                resolver.Register()
                    .Lifetime(Wellknown.Lifetimes.PerContainer)
                    .Contract<ICache<Type, IResolverFactory>>()
                    .AsFactoryMethod(ctx => new Cache<Type, IResolverFactory>());

            yield return
                resolver.Register()
                    .Lifetime(Wellknown.Lifetimes.PerContainer)
                    .Contract<ICache<ICompositeKey, RegistrationItem>>()
                    .AsFactoryMethod(ctx => new Cache<ICompositeKey, RegistrationItem>());
        }
    }
}