namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal sealed class RegistrationResult<TContainer> : IRegistrationResult<TContainer> where TContainer : IContainer
    {
        private readonly Registration<TContainer> _registration;
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public RegistrationResult([NotNull] Registration<TContainer> registration)
        {
#if DEBUG
            if (registration == null) throw new ArgumentNullException(nameof(registration));
#endif
            _registration = registration;
        }

        internal Registration<TContainer> Registration => _registration;

        public void AddResource(IDisposable resource)
        {
            _resources.Add(resource);
        }

        public IRegistration<TContainer> And()
        {
            return _registration.New();
        }

        public TContainer ToSelf()
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
