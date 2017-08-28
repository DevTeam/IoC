namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Contracts;

    internal sealed class RegistrationItem : IDisposable
    {
        private IEnumerable<IDisposable> _resources;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public RegistrationItem(
            RegistryContext registryContext,
            [NotNull] IEnumerable<IDisposable> resources)
        {
#if DEBUG
            if (resources == null) throw new ArgumentNullException(nameof(resources));
#endif
            _resources = resources;
            RegistryContext = registryContext;
            InstanceFactory = new LifetimesFactory(registryContext.Extensions.OfType<ILifetime>().ToList());
            foreach (var extension in registryContext.Extensions)
            {
                switch (extension)
                {
                    case IScope scope:
                        Scope = scope;
                        break;

                    case IKeyComparer keyComparer:
                        KeyComparer = keyComparer;
                        break;
                }
            }
        }

        public RegistryContext RegistryContext { [NotNull] get; }

        public LifetimesFactory InstanceFactory { [NotNull] get; }

        public IScope Scope { [CanBeNull] get; }

        public IKeyComparer KeyComparer { [CanBeNull] get; }

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