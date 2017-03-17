#if NET35
// ReSharper disable once CheckNamespace
namespace System
{
    public interface IObservable<out T>
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

}
#endif
