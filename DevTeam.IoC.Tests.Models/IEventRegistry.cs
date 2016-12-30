namespace DevTeam.IoC.Tests.Models
{
    public interface IEventRegistry
    {
        void RegisterEvent<TEvent>();
    }
}
