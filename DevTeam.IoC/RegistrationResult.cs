namespace DevTeam.IoC
{
    using System;
    using Contracts;

    public class RegistrationResult<T> : IRegistrationResult<T> where T : IContainer
    {
        private readonly IRegistration<T> _registration;
        private readonly Func<IDisposable> _registrationFactory;

        public RegistrationResult(
            [NotNull] IRegistration<T> registration,
            [NotNull] Func<IDisposable> registrationFactory)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (registrationFactory == null) throw new ArgumentNullException(nameof(registrationFactory));
            _registration = registration;
            _registrationFactory = registrationFactory;
        }

        public IDisposable Apply()
        {
            return _registrationFactory();
        }

        public T ToSelf()
        {
            return _registration.ToSelf(_registrationFactory());
        }
    }
}
