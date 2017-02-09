﻿namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface IConfiguring : IEnumerable<IConfiguration>
    {
        [NotNull]
        IConfiguring DependsOn([NotNull][ItemNotNull] params IConfiguration[] configurations);

        [NotNull]
        IConfiguring DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new();

        [NotNull]
        IConfiguring DependsOn([NotNull] params Wellknown.Features[] features);

        [NotNull]
        IConfiguring DependsOn([NotNull] Type configurationType, [NotNull] string description);

        [NotNull]
        IConfiguring DependsOn<TConfiguration>([NotNull] string description) where TConfiguration: IConfiguration, new();

        [NotNull]
        IConfiguring DependsOn(params Assembly[] assemblies);

        [NotNull]
        IDisposable Apply();
    }
}
