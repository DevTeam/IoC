namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal interface IObservable
    {
        [NotNull]
        IObservable<T> GetEventSource<T>() where T : IEvent;
    }
}
