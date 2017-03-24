namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class Subject<T>: IObservable<T>, IObserver<T>
    {
        [CanBeNull] private readonly Action<int> _onChange;
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public Subject([CanBeNull] Action<int> onChange = null)
        {
            _onChange = onChange;
        }

        public IDisposable Subscribe([NotNull] IObserver<T> observer)
        {
#if DEBUG
            if (observer == null) throw new ArgumentNullException(nameof(observer));
#endif
            _observers.Add(observer);
            _onChange?.Invoke(_observers.Count);
            return new Disposable(() =>
            {
                _observers.Remove(observer);
                _onChange?.Invoke(_observers.Count);
            });
        }

        public void OnCompleted()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        public void OnError([NotNull] Exception error)
        {
#if DEBUG
            if (error == null) throw new ArgumentNullException(nameof(error));
#endif
            foreach (var observer in _observers)
            {
                observer.OnError(error);
            }
        }

        public void OnNext([NotNull] T value)
        {
#if DEBUG
            if (value == null) throw new ArgumentNullException(nameof(value));
#endif
            foreach (var observer in _observers)
            {
                observer.OnNext(value);
            }
        }
    }
}
