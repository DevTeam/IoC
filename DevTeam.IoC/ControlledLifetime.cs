namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class ControlledLifetime: ILifetime
    {
        private readonly HashSet<object> _instances = new HashSet<object>();

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (!lifetimeEnumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }

            lock (_instances)
            {
                var instance = lifetimeEnumerator.Current.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
                _instances.Add(instance);
                return instance;
            }
        }

        public void Dispose()
        {
            lock (_instances)
            {
                foreach (var disposable in _instances.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }

                _instances.Clear();
            }
        }
    }
}
