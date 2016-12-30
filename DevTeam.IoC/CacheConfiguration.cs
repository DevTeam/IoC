namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class CacheConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new CacheConfiguration();

        private CacheConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return LifetimesConfiguration.Shared;
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
