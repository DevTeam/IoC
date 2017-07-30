﻿namespace DevTeam.IoC
{
    using Contracts;

    internal interface IRegistrationEvent : IEvent
    {
        IKey Key { [NotNull] get; }

        // ReSharper disable once UnusedMemberInSuper.Global
        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
