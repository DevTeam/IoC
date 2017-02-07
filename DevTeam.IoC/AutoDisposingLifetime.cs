namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class AutoDisposingLifetime: ILifetime
    {
        private readonly HashSet<IDisposable> _instances = new HashSet<IDisposable>();

        internal int Count => _instances.Count;

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (!lifetimeEnumerator.MoveNext())
            {
                throw new InvalidOperationException();
            }

            lock (_instances)
            {
                var instance = lifetimeEnumerator.Current.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
                var disposable = instance as IDisposable;
                if (disposable != null)
                {
                    _instances.Add(disposable);
                }

                return instance;
            }
        }

        public void Dispose()
        {
            IDisposable[] instances;
            lock (_instances)
            {
                instances = _instances.ToArray();
                _instances.Clear();
            }

            foreach (var disposable in instances)
            {
                disposable.Dispose();
            }
        }
    }
}
