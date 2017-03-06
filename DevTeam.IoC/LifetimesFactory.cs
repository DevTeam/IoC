namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class LifetimesFactory: IResolverFactory, IDisposable
    {
        private readonly IList<ILifetime> _lifetimes;
        private readonly Func<ICreationContext, object> _factory;

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

        public object Create(ICreationContext creationContext)
        {
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            return _factory(creationContext);
        }

        public void Dispose()
        {
            foreach (var lifetime in _lifetimes)
            {
                lifetime.Dispose();
            }
        }

        private static object CreateSimple(ICreationContext creationContext)
        {
            using (new LifetimeContext())
            {
                return creationContext.ResolverContext.RegistryContext.InstanceFactory.Create(creationContext);
            }
        }

        private object CreateUsingLifetimes(ICreationContext creationContext)
        {
            using (var lifetimesEnumerator = _lifetimes.GetEnumerator())
            {
                lifetimesEnumerator.MoveNext();
                using (var lifetimeContext = new LifetimeContext())
                {
                    return lifetimesEnumerator.Current.Create(lifetimeContext, creationContext, lifetimesEnumerator);
                }
            }
        }
    }
}
