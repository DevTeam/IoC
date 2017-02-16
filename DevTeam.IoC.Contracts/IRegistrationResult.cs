namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<T> where T : IContainer
    {
        [NotNull]
        IDisposable Apply();

        [NotNull]
        T ToSelf();

        [NotNull]
        IRegistration<T> And();
    }
}