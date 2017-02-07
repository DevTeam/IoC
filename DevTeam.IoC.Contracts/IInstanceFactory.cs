namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IInstanceFactory
    {
        object Create(params object[] args);
    }
}
