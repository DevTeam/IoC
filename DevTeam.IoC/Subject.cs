namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class Subject<T>: IObservable<T>, IObserver<T>
    {
        private readonly Action<int> _onChange;
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public Subject([CanBeNull] Action<int> onChange = null)
        {
            _onChange = onChange;
        }

        public IDisposable Subscribe([NotNull] IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
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
            if (error == null) throw new ArgumentNullException(nameof(error));
            foreach (var observer in _observers)
            {
                observer.OnError(error);
            }
        }

        public void OnNext([NotNull] T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            foreach (var observer in _observers)
            {
                observer.OnNext(value);
            }
        }
    }
}
