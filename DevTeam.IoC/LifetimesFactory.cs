namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class LifetimesFactory: IResolverFactory, IDisposable
    {
        private readonly IList<ILifetime> _lifetimes;
        private readonly Func<IResolverContext, object> _factory;

        public LifetimesFactory(IList<ILifetime> lifetimes)
        {
            if (lifetimes == null) throw new ArgumentNullException(nameof(lifetimes));
            _lifetimes = lifetimes;
            if (_lifetimes.Count > 0)
            {
                _lifetimes.Add(TransientLifetime.Shared);
                _factory = CreateUsingLifetimes;
            }
            else
            {
                _factory = CreateSimple;
            }
        }

        public object Create(IResolverContext resolverContext)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            return _factory(resolverContext);
        }

        public void Dispose()
        {
            foreach (var lifetime in _lifetimes)
            {
                lifetime.Dispose();
            }
        }

        private static object CreateSimple(IResolverContext resolverContext)
        {
            using (new LifetimeContext())
            {
                return resolverContext.RegistryContext.InstanceFactory.Create(resolverContext);
            }
        }

        private object CreateUsingLifetimes(IResolverContext resolverContext)
        {
            using (var lifetimesEnumerator = _lifetimes.GetEnumerator())
            {
                lifetimesEnumerator.MoveNext();
                using (var lifetimeContext = new LifetimeContext())
                {
                    return lifetimesEnumerator.Current.Create(lifetimeContext, resolverContext, lifetimesEnumerator);
                }
            }
        }
    }
}
