namespace DevTeam.IoC.Tests.Models
{
    using System;

    internal class Subscription : IDisposable
    {
        private readonly Action _disposeAction;

        public Subscription(Action disposeAction)
        {
            if (disposeAction == null) throw new ArgumentNullException(nameof(disposeAction));
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}