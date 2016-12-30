namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dto;

    internal class DtoConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new DtoConfiguration();

        private DtoConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return LifetimesConfiguration.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
           yield return
                resolver
                .Register()
                .Contract<ITypeResolver>()
                .AsAutowiring<TypeResolver>();

            yield return
                resolver
                .Register()
                .Contract<IConfiguration>()
                .State<IConfigurationDto>(0)
                .AsAutowiring<ConfigurationDtoAdapter>();

            yield return
                resolver
                .Register()
                .State<string>(0)
                .Contract<IConfigurationDescriptionDto>()
                .AsFactoryMethod(ctx => new ConfigurationDescriptionDto(ctx.GetState<string>(0)));
        }
    }
}
