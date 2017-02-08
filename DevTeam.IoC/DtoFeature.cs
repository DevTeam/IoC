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

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
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
                .AsFactoryMethod(ctx =>
                    {
                        var description = ctx.GetState<string>(0);
                        if (string.IsNullOrWhiteSpace(description))
                        {
                            throw new InvalidOperationException("Empty description is not allowed.");
                        }

                        // ReSharper disable once AssignNullToNotNullAttribute
                        return new ConfigurationDescriptionDto(description);
                    });
        }
    }
}
