namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface IConfiguring<out T> : IEnumerable<IConfiguration>
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

        IConfiguring<T> Register([NotNull] Func<IRegistration, IDisposable> registration);

        [NotNull]
        IDisposable Create();

        [NotNull]
        T Apply();
    }
}
