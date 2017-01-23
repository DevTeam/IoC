namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IConfiguring : IEnumerable<IConfiguration>
    {
        IConfiguring DependsOn(params IConfiguration[] configurations);

        IConfiguring DependsOn<TConfiguration>() where TConfiguration : IConfiguration, new();

        IConfiguring DependsOn(params Wellknown.Configurations[] configurations);

        IConfiguring DependsOn(Type configurationType, string description);

        IConfiguring DependsOn<TConfiguration>(string description) where TConfiguration: IConfiguration, new();

        IDisposable Apply();
    }
}
