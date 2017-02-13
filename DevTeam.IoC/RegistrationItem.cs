﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class RegistrationItem : IDisposable
    {
        private IEnumerable<IDisposable> _resources;

        public RegistrationItem(IRegistryContext registryContext, LifetimesFactory factory, IEnumerable<IDisposable> resources)
        {
            _resources = resources;
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            RegistryContext = registryContext;
            InstanceFactory = factory;
            Scope = registryContext.Extensions.OfType<IScope>().SingleOrDefault();
            Key = new object();
        }

        public IRegistryContext RegistryContext { get; }

        public LifetimesFactory InstanceFactory { get; }

        public IScope Scope { get; }

        public object Key { get; }

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