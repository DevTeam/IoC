namespace DevTeam.IoC.Tests.Models
{
    using System;

    internal interface ITimer
    {
        IDisposable Subscribe(Action tickAction);
    }
}
