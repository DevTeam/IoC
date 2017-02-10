namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IFluent
    {
        bool TryGetRegistry<T>([NotNull] T resolver, out IRegistry registry)  where T : IResolver, IRegistry;

        [NotNull]
        IConfiguring<T> Configure<T>([NotNull] T resolver)  where T : IResolver, IRegistry;

        [NotNull]
        IRegistration<T> Register<T>([NotNull] T resolver)  where T : IResolver, IRegistry;

        [NotNull]
        IResolving<T> Resolve<T>([NotNull] T resolver)  where T : IResolver;
    }
}
