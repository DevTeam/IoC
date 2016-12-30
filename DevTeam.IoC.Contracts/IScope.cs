namespace DevTeam.IoC.Contracts
{
    public interface IScope: IExtension
    {
        bool AllowsRegistration(IRegistryContext context);

        bool AllowsResolving(IResolverContext context);
    }
}
