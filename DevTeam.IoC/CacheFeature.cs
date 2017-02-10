namespace DevTeam.IoC
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

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return
                container.Register()
                    .Lifetime(Wellknown.Lifetime.PerContainer)
                    .Contract<ICache<Type, IResolverFactory>>()
                    .FactoryMethod(ctx => new Cache<Type, IResolverFactory>());

            yield return
                container.Register()
                    .Lifetime(Wellknown.Lifetime.PerContainer)
                    .Contract<ICache<ICompositeKey, RegistrationItem>>()
                    .FactoryMethod(ctx => new Cache<ICompositeKey, RegistrationItem>());
        }
    }
}
