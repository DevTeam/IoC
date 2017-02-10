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
            rootContainer.Configure().DependsOn(Wellknown.Feature.Dto).Own();
            rootContainer.Configure().DependsOn(Wellknown.Feature.Scopes).Own();
            _container = rootContainer.CreateChild();
            _container.Register().Contract<ITypeResolver>().FactoryMethod(ctx => _typeResolver);
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
            using (_container.Register().Contract<IReferenceDescriptionResolver>().FactoryMethod(ctx => referenceDescriptionResolver.Object))
            using (_container.Register().Contract<IConfigurationDto>().Tag(typeof(MyConfiguration)).State<IConfigurationDescriptionDto>(0).FactoryMethod(ctx => configurationDepDto))
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
        public void ShouldGetDependenciesWhenDependencyFeature(Wellknown.Feature feature)
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
                    Tag = new TagDto { Value = "10", TypeName = typeof(int).FullName },
                    Statements = new IConfigurationStatementDto[]
                    {
                        new RegisterDto
                        {
                            Keys = new IRegisterStatementDto []
                            {
                                new ContractDto { Contract = new []{ typeof(string).FullName }},
                                new ScopeDto { Scope = Wellknown.Scope.Global }
                            },
                            FactoryMethodName = $"{typeof(MyFactory).FullName}.{nameof(MyFactory.Create)}"
                        }
                    }
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var registrationContainerName = _container.Resolve().Instance<string>();

            // Then
            registrationContainerName.ShouldBe("10");
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

        [Test]
        public void ShouldApplyWhenRegisterAutowiringTypeName()
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
                        new ContractDto { Contract = new []{ typeof(ISimpleService).FullName }}
                    },
                    AutowiringTypeName = typeof(SimpleService).FullName
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Instance<ISimpleService>();

            // Then
            instance.ShouldBeOfType<SimpleService>();
        }

        [Test]
        public void ShouldApplyWhenRegisterUsingContract()
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
                        new ContractDto { Contract = new []{ typeof(ISimpleService).FullName, typeof(IDisposable).FullName }}
                    },
                    AutowiringTypeName = typeof(SimpleService).FullName
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Contract(typeof(IDisposable), typeof(ISimpleService)).Instance<ISimpleService>();

            // Then
            instance.ShouldBeOfType<SimpleService>();
        }

        [Test]
        public void ShouldApplyWhenRegisterUsingTags()
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
                        new ContractDto { Contract = new []{ typeof(ISimpleService).FullName }},
                        new TagDto { Value = "abc" },
                        new TagDto { Value = "33", TypeName = typeof(int).FullName },
                        new TagDto { Value = "xyz", TypeName = typeof(string).FullName }
                    },
                    AutowiringTypeName = typeof(SimpleService).FullName
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance1 = _container.Resolve().Tag("abc").Tag("xyz").Tag(33).Instance<ISimpleService>();
            var instance2 = _container.Resolve().Tag(33).Tag("abc").Tag("xyz").Instance<ISimpleService>();
            var instance3 = _container.Resolve().Tag("xyz").Tag(33).Tag("abc").Instance<ISimpleService>();

            // Then
            instance1.ShouldBeOfType<SimpleService>();
            instance2.ShouldBeOfType<SimpleService>();
            instance3.ShouldBeOfType<SimpleService>();
        }

        [Test]
        public void ShouldApplyWhenRegisterUsingState()
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
                        new ContractDto { Contract = new []{ typeof(ISimpleService).FullName }},
                        new TagDto { Value = "abc" },
                        new StateDto { Index = 0, StateTypeName = typeof(string).FullName } 
                    },
                    AutowiringTypeName = typeof(SimpleService).FullName
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Tag("abc").State<string>(0).Instance<ISimpleService>("xyz");

            // Then
            instance.ShouldBeOfType<SimpleService>();
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
            public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
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

            // ReSharper disable once UnusedParameter.Local
            public static string CreateAbcString(IResolverContext ctx)
            {
                return "abc";
            }
        }

        private class SimpleService : ISimpleService, IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
