namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IScope: IExtension
    {
        bool AllowRegistration(RegistryContext context, [NotNull] IContainer targetContainer);

        bool AllowResolving(ResolverContext context);
    }
}
