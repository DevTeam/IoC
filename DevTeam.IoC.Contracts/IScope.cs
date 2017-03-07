namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IScope: IExtension
    {
        bool AllowRegistration([NotNull] IRegistryContext context, [NotNull] IContainer targetContainer);

        bool AllowResolving([NotNull] IResolverContext context);
    }
}
