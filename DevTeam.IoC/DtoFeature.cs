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

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return LifetimesFeature.Shared;
            yield return ChildContainersFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return
                resolver
                .Register()
                .Contract<ITypeResolver>()
                .Autowiring<TypeResolver>();

            yield return
                resolver
                .Register()
                .Contract<IConfiguration>()
                .State<IConfigurationDto>(0)
                .Autowiring<ConfigurationDtoAdapter>();

            yield return
                resolver
                .Register()
                .State<string>(0)
                .Contract<IConfigurationDescriptionDto>()
                .FactoryMethod(ctx => new ConfigurationDescriptionDto(ctx.GetState<string>(0)));
        }
    }
}
