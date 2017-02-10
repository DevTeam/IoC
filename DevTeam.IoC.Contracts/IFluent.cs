namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IFluent
    {
        [NotNull]
        IConfiguring<T> Configure<T>([NotNull] T resolver)  where T : IResolver, IRegistry;

        [NotNull]
        IRegistration<T> Register<T>([NotNull] T resolver)  where T : IResolver, IRegistry;

        [NotNull]
        IResolving<T> Resolve<T>([NotNull] T resolver)  where T : IResolver;
    }
}
