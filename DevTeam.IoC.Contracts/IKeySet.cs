namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    public interface IKeySet<T>: IEnumerable<T>
        where T : IKey
    {
        int Count { get; }

        bool IsEqual(IKeySet<T> keys);

        bool IsIntersecting(IKeySet<T> keys);
    }
}
