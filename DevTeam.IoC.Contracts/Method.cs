﻿namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    [NotNull] public delegate void Method([NotNull] object instance, [NotNull][ItemCanBeNull] params object[] args);
}
