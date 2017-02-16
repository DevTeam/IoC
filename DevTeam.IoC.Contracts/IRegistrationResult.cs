namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<T> where T : IContainer
    {
        [NotNull]
        IDisposable Create();

        [NotNull]
        T ToSelf();

        [NotNull]
        IRegistration<T> And();
    }
}