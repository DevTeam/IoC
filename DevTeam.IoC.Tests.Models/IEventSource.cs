namespace DevTeam.IoC.Tests.Models
{
    using System;

    internal interface IEventSource<out TEvent>
    {
        IDisposable Subscribe(IEventListener<TEvent> listener);
    }
}
