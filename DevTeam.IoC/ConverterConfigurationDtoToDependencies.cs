namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConverterConfigurationDtoToDependencies: IConverter<IConfigurationDto, IEnumerable<IConfiguration>, IContainer>
    {
        [NotNull] private readonly ITypeResolver _typeResolver;

        public ConverterConfigurationDtoToDependencies(
            [NotNull] ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public bool TryConvert(IConfigurationDto configurationDto, out IEnumerable<IConfiguration> value, IContainer container)
        {
            value = Convert(configurationDto, container);
            return true;
        }

        private IEnumerable<IConfiguration> Convert(IConfigurationDto configurationDto, IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();
            var references = new List<Assembly>();
            var usings = new List<string>();
            foreach (var configurationStatement in configurationDto)
            {
                Type configurationType;
                switch (configurationStatement)
                {
                    case IReferenceDto referenceDto:
                        references.Add(Assembly.Load(new AssemblyName(referenceDto.Reference)));
                        break;

                    case IUsingDto usingDto:
                        usings.Add(usingDto.Using);
                        break;

                    case IDependencyFeatureDto dependencyFeatureDto:
                        yield return container.Feature(dependencyFeatureDto.Feature);
                        break;

                    case IDependencyConfigurationDto dependencyConfigurationDto:
                        if (!_typeResolver.TryResolveType(references, usings, dependencyConfigurationDto.ConfigurationTypeName, out configurationType) || !reflection.GetType(typeof(IConfiguration)).IsAssignableFrom(reflection.GetType(configurationType)))
                        {
                            throw new Exception($"Invalid configuration type {configurationType}");
                        }

                        using (var childContainer = container.CreateChild()
                                .Register().Contract<IConfiguration>().Autowiring(configurationType).ToSelf())
                        {
                            yield return childContainer.Resolve().Instance<IConfiguration>();
                        }
                        break;

                    case IDependencyAssemblyDto dependencyAssemblyDto:
                        var assembly = Assembly.Load(new AssemblyName(dependencyAssemblyDto.AssemblyName));
                        yield return container.Resolve().State<Assembly>(0).Instance<IConfiguration>(assembly);
                        break;

                    case IDependencyReferenceDto dependencyReferenceDto:
                        if (!_typeResolver.TryResolveType(references, usings, dependencyReferenceDto.ConfigurationTypeName, out configurationType) || !reflection.GetType(typeof(IConfiguration)).IsAssignableFrom(reflection.GetType(configurationType)))
                        {
                            throw new Exception($"Invalid configuration type {configurationType}");
                        }

                        using (var childContainer = container.CreateChild())
                        {
                            var referenceDescriptionResolver = childContainer.Resolve().Instance<IReferenceDescriptionResolver>();
                            var reference = referenceDescriptionResolver.ResolveReference(dependencyReferenceDto.Reference);
                            var configurationDescriptionDto = childContainer.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(reference);
                            var nestedConfigurationDto = childContainer.Resolve().Tag(configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
                            yield return container.Resolve().Instance<IConfiguration>(nestedConfigurationDto);
                        }
                        break;
                }
            }
        }
    }
}
