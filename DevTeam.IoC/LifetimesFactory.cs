namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal sealed class LifetimesFactory: IInstanceFactory, IDisposable
    {
        private readonly IList<ILifetime> _lifetimes;
        private readonly Func<CreationContext, object> _factory;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
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

        public object Create(CreationContext creationContext)
        {
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
        private object CreateSimple(CreationContext creationContext)
        {
            using (new LifetimeContext())
            {
                return creationContext.ResolverContext.RegistryContext.InstanceFactory.Create(creationContext);
            }
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private object CreateUsingLifetimes(CreationContext creationContext)
        {
            using (var lifetimesEnumerator = _lifetimes.GetEnumerator())
            {
                if (!lifetimesEnumerator.MoveNext())
                {
                    throw new ContainerException($"Invalid chain of lifetimes.\nDetails:\n{creationContext}");
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
