namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IScope: IExtension
    {
        bool AllowsRegistration(IRegistryContext context);

        bool AllowsResolving(IResolverContext context);
    }
}
