namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IFluent
    {
        bool TryGetRegistry(out IRegistry registry);

        [NotNull]
        IConfiguring Configure([NotNull] IResolver resolver);

        [NotNull]
        IRegistration Register();

        [NotNull]
        IResolving Resolve();
    }
}
