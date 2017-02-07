namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IFluent
    {
        bool TryGetRegistry(out IRegistry registry);

        IConfiguring Configure(IResolver resolver);

        IRegistration Register();

        IResolving Resolve();
    }
}
