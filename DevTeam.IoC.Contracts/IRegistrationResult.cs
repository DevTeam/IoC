namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<out T>: IDisposable where T : IResolver
    {
        [NotNull]
        IRegistration<T> And();

        [NotNull]
        T ToSelf();
    }
}