namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IContainer: IResolver, IRegistry, IDisposable
    {
        object Tag { [CanBeNull] get; }

        IEnumerable<ICompositeKey> Registrations { [NotNull] get; }
    }
}
