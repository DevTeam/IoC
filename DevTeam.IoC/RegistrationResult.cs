namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class RegistrationResult<T> : IRegistrationResult<T> where T : IContainer
    {
        private readonly Registration<T> _registration;
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public RegistrationResult([NotNull] Registration<T> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
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

        public IDisposable Apply()
        {
            return new CompositeDisposable(_resources);
        }

        public T ToSelf()
        {
            return _registration.ToSelf(Apply());
        }
    }
}
