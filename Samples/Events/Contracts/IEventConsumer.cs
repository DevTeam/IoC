namespace Contracts
{
    using System;

    public interface IEventConsumer<in T>: IObserver<T>
    {
    }
}
