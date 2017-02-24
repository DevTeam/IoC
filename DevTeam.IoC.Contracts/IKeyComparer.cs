namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IKeyComparer: IEqualityComparer<IKey>, IExtension
    {
    }
}
