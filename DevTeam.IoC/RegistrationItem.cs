namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class RegistrationItem : IDisposable
    {
        private readonly IScope _scope;
        private readonly IKeyComparer _keyComparer;
        private IEnumerable<IDisposable> _resources;

        public RegistrationItem(
            [NotNull] IRegistryContext registryContext,
            [NotNull] IEnumerable<IDisposable> resources)
        {
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            _resources = resources;
            RegistryContext = registryContext;
            InstanceFactory = new LifetimesFactory(registryContext.Extensions.OfType<ILifetime>().ToList());
            Key = new object();
            TryGetExtension(out _scope);
            TryGetExtension(out _keyComparer);
        }

        public IRegistryContext RegistryContext { [NotNull] get; }

        public LifetimesFactory InstanceFactory { [NotNull] get; }

        [CanBeNull]
        public IScope Scope => _scope;

        [CanBeNull]
        public IKeyComparer KeyComparer => _keyComparer;

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

        private bool TryGetExtension<TContract>(out TContract instance)
            where TContract : class, IExtension
        {
            instance = RegistryContext.Extensions.OfType<TContract>().SingleOrDefault();
            return instance != default(TContract);
        }
    }
}