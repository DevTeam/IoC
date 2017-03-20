namespace DevTeam.IoC
{
    using Contracts;

    internal interface IEvent
    {
        EventStage Stage { [NotNull] get; }

        EventAction Action { [NotNull] get; }
    }
}
