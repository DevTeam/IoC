#if NET35
// ReSharper disable once CheckNamespace
namespace System
{
    using System;

    public interface IObserver<in T>
    {
        void OnNext(T value);

        void OnError(Exception error);

        void OnCompleted();
    }
}
#endif
