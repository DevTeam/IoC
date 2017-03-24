﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class RegistrationResult<T> : IRegistrationResult<T> where T : IContainer
    {
        private readonly Registration<T> _registration;
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public RegistrationResult([NotNull] Registration<T> registration)
        {
#if DEBUG
            if (registration == null) throw new ArgumentNullException(nameof(registration));
#endif
            _registration = registration;
        }

        public void AddResource(IDisposable resource)
        {
            _resources.Add(resource);
        }

        public IRegistration<T> And()
        {
            return _registration;
        }

        public T ToSelf()
        {
            return _registration.ToSelf(this);
        }

        public void Dispose()
        {
            foreach (var resource in _resources)
            {
                resource.Dispose();
            }
        }
    }
}
