namespace DevTeam.IoC
{
    using Contracts;

    public interface IEvent
    {
        EventStage Stage { [NotNull] get; }
    }
}
