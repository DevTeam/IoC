namespace DevTeam.IoC
{
    using Contracts;

    internal interface IRegistrationEvent : IEvent
    {
        IKey Key { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
