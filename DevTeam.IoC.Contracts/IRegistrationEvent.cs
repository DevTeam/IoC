namespace DevTeam.IoC.Contracts
{
    public interface IRegistrationEvent : IEvent
    {
        IKey Key { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
