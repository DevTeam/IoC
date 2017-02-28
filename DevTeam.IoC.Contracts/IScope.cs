namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IScope: IExtension
    {
        bool AllowRegistration([NotNull] IRegistryContext context);

        bool AllowResolving([NotNull] IResolverContext context);
    }
}
