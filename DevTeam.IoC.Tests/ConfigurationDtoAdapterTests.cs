namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configurations.Json;
    using Contracts;
    using Contracts.Dto;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ConfigurationDtoAdapterTests
    {
        private MyTypeResolver _typeResolver;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _typeResolver = new MyTypeResolver();
            var rootContainer = new Container();
            rootContainer.Configure().DependsOn(Wellknown.Features.Dto).Apply();
            rootContainer.Configure().DependsOn(Wellknown.Features.Scopes).Apply();
            _container = rootContainer.CreateChild();
            _container.Register().Contract<ITypeResolver>().AsFactoryMethod(ctx => _typeResolver);
        }

        [Test]
        public void ShouldGetDependenciesWhenEmpty()
        {
            // Given
            
            // When
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);
            var dependencies = configuration.GetDependencies(_container).ToArray();

            // Then
            dependencies.Length.ShouldBe(0);
        }

        [Test]
        public void ShouldGetDependenciesWhenUsingAndReferences()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(new UsingDto { Using = "abc" });
            configurationDto.Add(new ReferenceDto { Reference = "xyz" });
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.GetDependencies(_container).ToArray();

            // Then
            _typeResolver.UsingStatement.ShouldBe(new[] { "abc" });
            _typeResolver.References.ShouldBe(new[] { "xyz" });
        }

        [Test]
        public void ShouldGetDependenciesWhenDependencyConfiguration()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(new DependencyConfigurationDto { ConfigurationTypeName = typeof(MyConfiguration).FullName });
            var dependencies = configuration.GetDependencies(_container).ToArray();

            // Then
            dependencies.Length.ShouldBe(1);
            dependencies[0].ShouldBeOfType<MyConfiguration>();
        }

        [Test]
        public void ShouldGetDependenciesWhenDependencyReference()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);
            var referenceDescriptionResolver = new Mock<IReferenceDescriptionResolver>();
            referenceDescriptionResolver.Setup(i => i.ResolveReference("ref")).Returns("ref data");
            var configurationDepDto = Mock.Of<IConfigurationDto>();
            using (_container.Register().Contract<IReferenceDescriptionResolver>().AsFactoryMethod(ctx => referenceDescriptionResolver.Object))
            using (_container.Register().Contract<IConfigurationDto>().Tag(typeof(MyConfiguration)).State<IConfigurationDescriptionDto>(0).AsFactoryMethod(ctx => configurationDepDto))
            {

                // When
                configurationDto.Add(new DependencyReferenceDto { Reference = "ref", ConfigurationTypeName = typeof(MyConfiguration).FullName });
                var dependencies = configuration.GetDependencies(_container).ToArray();

                // Then
                dependencies.Length.ShouldBe(1);
                dependencies[0].ShouldBeOfType<ConfigurationDtoAdapter>();
                ((ConfigurationDtoAdapter)dependencies[0]).ConfigurationDto.ShouldBe(configurationDepDto);
            }
        }

        [Test]
        [Theory]
        public void ShouldGetDependenciesWhenDependencyFeature(Wellknown.Features feature)
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(new DependencyFeatureDto { Feature = feature });
            var dependencies = configuration.GetDependencies(_container).ToArray();

            // Then
            dependencies.Length.ShouldBe(1);
            dependencies[0].ShouldBe(_container.Feature(feature));
        }

        [Test]
        public void ShouldApplyWhenUsingAndReferences()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(new UsingDto { Using = "abc" });
            configurationDto.Add(new ReferenceDto { Reference = "xyz" });
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();

            // Then
            _typeResolver.UsingStatement.ShouldBe(new[] { "abc" });
            _typeResolver.References.ShouldBe(new[] { "xyz" });
        }

        [Test]
        public void ShouldApplyWhenChildContainer()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(
                new ContainerDto
                {
                    Statements = new IConfigurationStatementDto[]
                    {
                        new RegisterDto
                        {
                            Keys = new IRegisterStatementDto []
                            {
                                new ContractDto { Contract = new []{ typeof(string).FullName }},
                                new ScopeDto { Scope = Wellknown.Scopes.Global }
                            },
                            FactoryMethodName = $"{typeof(MyFactory).FullName}.{nameof(MyFactory.Create)}"
                        }
                    },
                    Tag = new TagDto { Value = "10", TypeName = typeof(int).FullName }
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var registrationContainerName = _container.Resolve().Instance<string>();

            // Then
            registrationContainerName.ShouldBe("child");
        }

        [Test]
        [TestCase("{0}.{1}")]
        [TestCase("{0}. {1}")]
        [TestCase("  {0}   .   {1} ")]
        [TestCase("{0} : {1}")]
        [TestCase("{0}::{1}")]
        [TestCase("{0}->{1}")]
        public void ShouldApplyWhenRegisterFactoryMethod(string format)
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            configurationDto.Add(
                new RegisterDto
                {
                    Keys = new IRegisterStatementDto[]
                    {
                        new ContractDto { Contract = new []{ typeof(string).FullName }}
                    },
                    FactoryMethodName = string.Format(format, typeof(MyFactory).FullName, nameof(MyFactory.CreateAbcString))
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var registrationContainerName = _container.Resolve().Instance<string>();

            // Then
            registrationContainerName.ShouldBe("abc");
        }

        private ConfigurationDtoAdapter CreateInstance(IConfigurationDto configurationDto)
        {
            return new ConfigurationDtoAdapter(configurationDto);
        }

        private class MyTypeResolver: ITypeResolver
        {
            public IList<string> References { get; } = new List<string>();

            public IList<string> UsingStatement { get; } = new List<string>();

            public void AddReference(string reference)
            {
                References.Add(reference);
            }

            public void AddUsingStatement(string usingName)
            {
                UsingStatement.Add(usingName);
            }

            public bool TryResolveType(string typeName, out Type type)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    type = default(Type);
                    return false;
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                type = Type.GetType(typeName, false);
                return type != null;
            }
        }

        private class MyConfiguration : IConfiguration
        {
            public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IDisposable> Apply(IResolver resolver)
            {
                throw new NotImplementedException();
            }
        }

        private static class MyFactory
        {
            public static string Create(IResolverContext ctx)
            {
                return ctx.RegistryContext.Container.Tag?.ToString() ?? "null";
            }

            public static string CreateAbcString(IResolverContext ctx)
            {
                return "abc";
            }
        }
    }
}
