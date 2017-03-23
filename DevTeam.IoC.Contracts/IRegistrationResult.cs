namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<out T>: IDisposable where T : IContainer
    {
        [NotNull]
        IRegistration<T> And();

        [NotNull]
        T ToSelf();
    }
}