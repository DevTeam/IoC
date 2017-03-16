namespace DevTeam.IoC.Tests.Models
{
    public interface IEventRegistry
    {
        // ReSharper disable once UnusedMember.Global
        void RegisterEvent<TEvent>();
    }
}
