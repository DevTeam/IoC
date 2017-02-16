namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<out T> where T : IContainer
    {
        [NotNull]
        IRegistration<T> And();

        [NotNull]
        IDisposable Apply();

        [NotNull]
        T ToSelf();
    }
}