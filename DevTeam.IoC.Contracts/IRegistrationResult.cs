namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistrationResult<out T> where T : IContainer
    {
        [NotNull]
        IDisposable Apply();

        [NotNull]
        T ToSelf();
    }
}