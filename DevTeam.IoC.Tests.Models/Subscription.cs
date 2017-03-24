namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    internal sealed class Subscription : IDisposable
    {
        private readonly Action _disposeAction;

        public Subscription([NotNull] Action disposeAction)
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