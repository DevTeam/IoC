namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IFluent
    {
        bool TryGetRegistry(out IRegistry registry);

        [NotNull]
        IConfiguring<T> Configure<T>([NotNull] T resolver) where T: IResolver;

        [NotNull]
        IRegistration Register();

        [NotNull]
        IResolving Resolve();
    }
}
