namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    public interface IKeyComparer: IEqualityComparer<ICompositeKey>, IExtension
    {
    }
}
