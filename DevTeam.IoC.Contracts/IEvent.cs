﻿namespace DevTeam.IoC.Contracts
{
    public interface IEvent
    {
        EventStage Stage { [NotNull] get; }
    }
}