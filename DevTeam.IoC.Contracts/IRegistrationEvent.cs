namespace DevTeam.IoC.Contracts
{
    public interface IRegistrationEvent : IEvent
    {
        ICompositeKey Key { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
