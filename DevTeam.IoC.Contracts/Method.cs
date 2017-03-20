namespace DevTeam.IoC.Contracts
{
    [NotNull] public delegate void Method([NotNull] object instance, [NotNull][ItemCanBeNull] params object[] args);
}
