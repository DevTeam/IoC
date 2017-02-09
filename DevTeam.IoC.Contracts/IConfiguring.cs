namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface IConfiguring<T> : IEnumerable<IConfiguration>
        where T : IResolver
    {
        [NotNull]
        IConfiguring<T> DependsOn([NotNull][ItemNotNull] params IConfiguration[] configurations);

        [NotNull]
        IConfiguring<T> DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new();

        [NotNull]
        IConfiguring<T> DependsOn([NotNull] params Wellknown.Feature[] features);

        [NotNull]
        IConfiguring<T> DependsOn([NotNull] Type configurationType, [NotNull] string description);

        [NotNull]
        IConfiguring<T> DependsOn<TConfiguration>([NotNull] string description) where TConfiguration: IConfiguration, new();

        [NotNull]
        IConfiguring<T> DependsOn(params Assembly[] assemblies);

        [NotNull]
        IRegistration<T> Register();

        [NotNull]
        IDisposable Apply();

        [NotNull]
        T Include();
    }
}
