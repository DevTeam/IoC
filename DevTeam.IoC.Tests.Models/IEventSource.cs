namespace DevTeam.IoC.Tests.Models
{
    using System;

    internal interface IEventSource<out TEvent>
    {
        // ReSharper disable once UnusedMember.Global
        IDisposable Subscribe(IEventListener<TEvent> listener);
    }
}
