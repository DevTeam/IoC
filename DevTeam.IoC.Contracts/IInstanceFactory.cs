namespace DevTeam.IoC.Contracts
{
    public interface IInstanceFactory
    {
        object Create(params object[] args);
    }
}
