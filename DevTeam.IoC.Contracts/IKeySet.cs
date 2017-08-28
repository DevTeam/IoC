namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IKeySet<T>: IEnumerable<T>, IEquatable<IKeySet<T>>
        where T : IKey
    {
        int Count { get; }
    }
}
