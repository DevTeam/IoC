namespace DevTeam.IoC.Contracts
{
    public interface IEventRegistration : IEvent
    {
        RegistrationAction Action { [NotNull] get; }

        ICompositeKey Key { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
