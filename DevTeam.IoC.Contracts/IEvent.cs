namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IEvent
    {
        EventStage Stage { [NotNull] get; }

        EventAction Action { [NotNull] get; }
    }
}
