namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IScope: IExtension
    {
        bool AllowsRegistration([NotNull] IRegistryContext context);

        bool AllowsResolving([NotNull] IResolverContext context);
    }
}
