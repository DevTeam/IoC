namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal struct EventRegistration : IEventRegistration
    {
        public EventRegistration(
            EventStage stage,
            RegistrationAction action,
            [NotNull] ICompositeKey key,
            [NotNull] IRegistryContext registryContext)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
            Stage = stage;
            Action = action;
            Key = key;
            RegistryContext = registryContext;
        }

        public EventStage Stage { get; }

        public RegistrationAction Action { get; }

        public ICompositeKey Key { get; }

        public IRegistryContext RegistryContext { get; }

        public override string ToString()
        {
            return $"{nameof(EventRegistration)} [Stage: {Stage}, Action: {Action}, Key: {Key}, RegistryContext: {RegistryContext}]";
        }
    }
}
