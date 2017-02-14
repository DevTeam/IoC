namespace DevTeam.IoC.Contracts
{
    public interface IContainerEvent: IEvent
    {
        IContainer Container { [NotNull] get; }

        IContainer ParentContainer { [CanBeNull] get; }
    }
}
