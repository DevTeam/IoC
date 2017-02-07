namespace DevTeam.IoC
{
    using Contracts;

    [NotNull]
    internal delegate object InstanceFactoryMethod([NotNull][ItemCanBeNull] params object[] args);
}
