namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    [NotNull] public delegate object Constructor([NotNull][ItemCanBeNull] params object[] args);
}
