namespace DevTeam.IoC.Tests.Models
{
    internal interface IEventListener<in TEvent>
    {
        void OnEvent(IEvent<TEvent> e);
    }
}
