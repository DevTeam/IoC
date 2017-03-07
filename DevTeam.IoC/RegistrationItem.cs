namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class RegistrationItem : IDisposable
    {
        private IEnumerable<IDisposable> _resources;

        public RegistrationItem(
            [NotNull] IRegistryContext registryContext,
            [NotNull] IEnumerable<IDisposable> resources)
        {
#if DEBUG
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            if (resources == null) throw new ArgumentNullException(nameof(resources));
#endif
            _resources = resources;
            RegistryContext = registryContext;
            InstanceFactory = new LifetimesFactory(registryContext.Extensions.OfType<ILifetime>().ToList());
            Key = new object();
            foreach (var extension in registryContext.Extensions)
            {
                var scope = extension as IScope;
                if (scope != null)
                {
                    Scope = scope;
                }

                var keyComparer = extension as IKeyComparer;
                if (keyComparer != null)
                {
                    KeyComparer = keyComparer;
                }
            }
        }

        public IRegistryContext RegistryContext { [NotNull] get; }

        public LifetimesFactory InstanceFactory { [NotNull] get; }

        public IScope Scope { [CanBeNull] get; }

        public IKeyComparer KeyComparer { [CanBeNull] get; }

        public object Key { [NotNull] get; }

        public void Dispose()
        {
            foreach (var disposable in _resources)
            {
                disposable.Dispose();
            }
            _resources = Enumerable.Empty<IDisposable>();
            InstanceFactory.Dispose();
        }
    }
}