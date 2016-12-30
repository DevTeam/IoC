namespace DevTeam.IoC.Tests
{
    public interface IMultService<T>: ISimpleService, IGenericService<T>, IDisposableService
    {
    }
}
