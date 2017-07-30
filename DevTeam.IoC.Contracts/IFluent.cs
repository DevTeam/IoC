namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IFluent
    {
        [NotNull]
        IConfiguring<T> Configure<T>([NotNull] T container)  where T : IContainer;

        [NotNull]
        IRegistration<T> Register<T>([NotNull] T container)  where T : IContainer;

        [NotNull]
        IResolving<T> Resolve<T>([NotNull] T resolver)  where T : IResolver;
    }
}
