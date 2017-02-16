namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    [PublicAPI]
    public interface IConfiguring<out T>
         where T : IContainer
    {
        [NotNull]
        IConfiguring<T> Dependency([NotNull][ItemNotNull] params IConfiguration[] configurations);

        [NotNull]
        IConfiguring<T> Dependency<TConfiguration>() where TConfiguration : IConfiguration, new();

        [NotNull]
        IConfiguring<T> Dependency([NotNull] params Wellknown.Feature[] features);

        [NotNull]
        IConfiguring<T> Dependency([NotNull] Type configurationType, [NotNull] string description);

        [NotNull]
        IConfiguring<T> Dependency<TConfiguration>([NotNull] string description) where TConfiguration: IConfiguration, new();

        [NotNull]
        IConfiguring<T> Dependency(params Assembly[] assemblies);

        [NotNull]
        T ToSelf();

        IConfiguration Create();
    }
}
