namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IContainer: IResolver, IDisposable
    {
        object Tag { get; }

        IEnumerable<ICompositeKey> Registrations { get; }
    }
}
