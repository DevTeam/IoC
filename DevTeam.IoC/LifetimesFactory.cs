namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal sealed class LifetimesFactory: IInstanceFactory, IDisposable
    {
        private readonly IList<ILifetime> _lifetimes;
        private readonly Func<ICreationContext, object> _factory;

        public LifetimesFactory(IList<ILifetime> lifetimes)
        {
#if DEBUG
            if (lifetimes == null) throw new ArgumentNullException(nameof(lifetimes));
#endif
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
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            return _factory(creationContext);
        }

        public void Dispose()
        {
            foreach (var lifetime in _lifetimes)
            {
                lifetime.Dispose();
            }
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private object CreateSimple(ICreationContext creationContext)
        {
            using (new LifetimeContext())
            {
                return creationContext.ResolverContext.RegistryContext.InstanceFactory.Create(creationContext);
            }
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private object CreateUsingLifetimes(ICreationContext creationContext)
        {
            using (var lifetimesEnumerator = _lifetimes.GetEnumerator())
            {
                if (!lifetimesEnumerator.MoveNext())
                {
                    throw new InvalidOperationException("Invalid chain of factories");
                }

                using (var lifetimeContext = new LifetimeContext())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    return lifetimesEnumerator.Current.Create(lifetimeContext, creationContext, lifetimesEnumerator);
                }
            }
        }
    }
}
