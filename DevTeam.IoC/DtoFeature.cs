namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;

    internal class DtoFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new DtoFeature();

        private DtoFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LifetimesFeature.Shared;
            yield return ChildContainersFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return
                container
                .Register()
                .Contract<ITypeResolver>()
                .Autowiring<TypeResolver>()
                .Apply();

            yield return
                container
                .Register()
                .Contract<IConfiguration>()
                .State<IConfigurationDto>(0)
                .Autowiring<ConfigurationDtoAdapter>()
                .Apply();

            yield return
                container
                .Register()
                .State<string>(0)
                .Contract<IConfigurationDescriptionDto>()
                .FactoryMethod(ctx => new ConfigurationDescriptionDto(ctx.GetState<string>(0)))
                .Apply();
        }
    }
}
