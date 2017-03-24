namespace DevTeam.IoC
{
    using System;

    internal sealed class Disposable: IDisposable
    {
        private readonly Action _disposableAction;
        private readonly object _owner;

        public Disposable(Action disposableAction, object owner = null)
        {
#if DEBUG
            if (disposableAction == null) throw new ArgumentNullException(nameof(disposableAction));
#endif
            _disposableAction = disposableAction;
            _owner = owner;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose()
        {
            _disposableAction();
        }

        public override string ToString()
        {
            return $"{nameof(Disposable)} [Owner: {_owner ?? "unknown"}]";
        }
    }
}
