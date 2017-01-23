namespace Contracts
{
    using System;

    public interface IEventProducer<out T> : IObservable<T>
    {
    }
}
