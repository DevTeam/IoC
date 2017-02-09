namespace DevTeam.IoC
{
    using System;

    internal interface IInternalResourceStore: IDisposable
    {
        void AddResource(IDisposable resource);
    }
}
