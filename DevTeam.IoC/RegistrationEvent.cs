namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal struct RegistrationEvent : IRegistrationEvent
    {
        public RegistrationEvent(
            EventStage stage,
            EventAction action,
            [NotNull] IKey key,
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

        public EventAction Action { get; }

        public IKey Key { get; }

        public IRegistryContext RegistryContext { get; }

        public override string ToString()
        {
            return $"{nameof(RegistrationEvent)} [Stage: {Stage}, Action: {Action}, Key: {Key}, RegistryContext: {RegistryContext}]";
        }
    }
}
