namespace DevTeam.IoC.Tests.Models
{
    using System;

    internal interface IEventBroker: IDisposable
    {
        IDisposable RegisterSource<TEvent>(IEventSource<TEvent> source);

        IDisposable RegisterListener<TEvent>(IEventListener<TEvent> listener);
    }
}
