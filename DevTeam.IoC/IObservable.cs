#if NET35
// ReSharper disable once CheckNamespace
namespace System
{
    internal interface IObservable<out T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }
}
#endif
