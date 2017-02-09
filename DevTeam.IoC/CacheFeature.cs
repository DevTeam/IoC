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

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return
                resolver.Register()
                    .Lifetime(Wellknown.Lifetime.PerContainer)
                    .Contract<ICache<Type, IResolverFactory>>()
                    .FactoryMethod(ctx => new Cache<Type, IResolverFactory>());

            yield return
                resolver.Register()
                    .Lifetime(Wellknown.Lifetime.PerContainer)
                    .Contract<ICache<ICompositeKey, RegistrationItem>>()
                    .FactoryMethod(ctx => new Cache<ICompositeKey, RegistrationItem>());
        }
    }
}
