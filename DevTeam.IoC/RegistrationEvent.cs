namespace DevTeam.IoC
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal struct RegistrationEvent : IRegistrationEvent
    {
        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public RegistrationEvent(
            EventStage stage,
            EventAction action,
            [NotNull] IKey key,
            [NotNull] IRegistryContext registryContext)
        {
#if DEBUG
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (registryContext == null) throw new ArgumentNullException(nameof(registryContext));
#endif
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
