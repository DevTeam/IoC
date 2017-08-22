namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Configurations.Json;
    using Contracts;
    using Contracts.Dto;
    using Moq;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public class ConfigurationDtoAdapterTests
    {
        private readonly MyTypeResolver _typeResolver;
        private readonly IContainer _container;

        public ConfigurationDtoAdapterTests()
        {
            _typeResolver = new MyTypeResolver();
            var rootContainer = new Container();
            rootContainer.Configure().DependsOn(Wellknown.Feature.Dto).ToSelf();
            rootContainer.Configure().DependsOn(Wellknown.Feature.Scopes).ToSelf();
            _container = rootContainer.CreateChild();

        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void ShouldGetDependenciesWhenDependencyAssembly()
        {
            // Given
            var configurationDto = new ConfigurationDto();
            var configuration = CreateInstance(configurationDto);

            // When
            // ReSharper disable once PossibleMistakenCallToGetType.2
#if !NETCOREAPP1_0
            var assembly = typeof(MyConfiguration).GetType().Assembly;
#else
            var assembly = typeof(MyConfiguration).GetTypeInfo().Assembly;
#endif
            configurationDto.Add(new DependencyAssemblyDto { AssemblyName = assembly.FullName });
            var dependencies = configuration.GetDependencies(_container).ToArray();

            // Then
            dependencies.Length.ShouldBe(1);
            dependencies[0].ShouldBeOfType<ConfigurationFromAssembly>();
        }

        [Fact]
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

#if !NET35
        [Theory]
        [InlineData(Wellknown.Feature.Default)]
        [InlineData(Wellknown.Feature.ChildContainers)]
        [InlineData(Wellknown.Feature.Dto)]
        [InlineData(Wellknown.Feature.Enumerables)]
        [InlineData(Wellknown.Feature.KeyComparers)]
        [InlineData(Wellknown.Feature.Lifetimes)]
        [InlineData(Wellknown.Feature.Observables)]
        [InlineData(Wellknown.Feature.Resolvers)]
        [InlineData(Wellknown.Feature.Scopes)]
#if !NET35
        [InlineData(Wellknown.Feature.Tasks)]
#endif
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
#endif
        [Fact]
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

#if !NET35
        [Theory]
        [InlineData("{0}.{1}")]
        [InlineData("{0}. {1}")]
        [InlineData("  {0}   .   {1} ")]
        [InlineData("{0} : {1}")]
        [InlineData("{0}::{1}")]
        [InlineData("{0}->{1}")]
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
#endif

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void ShouldApplyWhenRegisterUsingConstructorParameters()
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
                        new StateDto { Index = 0, StateTypeName = typeof(int).FullName }
                    },
                    AutowiringTypeName = typeof(SimpleService2).FullName,
                    ConstructorParameters = new List<IParameterDto> { new ParameterDto { TypeName = typeof(int).FullName, State = new StateDto { Index = 0, StateTypeName = typeof(int).FullName } } }
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Instance<ISimpleService>(33);

            // Then
            instance.ShouldBeOfType<SimpleService2>();
            ((SimpleService2)instance).Val.ShouldBe(33);
        }

        [Fact]
        public void ShouldConfigureStateKeysWhenRegisterUsingConstructorParametersWithStates()
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
                    AutowiringTypeName = typeof(SimpleService2).FullName,
                    ConstructorParameters = new List<IParameterDto> { new ParameterDto { TypeName = typeof(int).FullName, State = new StateDto { Index = 0, StateTypeName = typeof(int).FullName } } }
                });

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Instance<ISimpleService>(33);

            // Then
            instance.ShouldBeOfType<SimpleService2>();
            ((SimpleService2)instance).Val.ShouldBe(33);
        }

        [Fact]
        public void ShouldApplyWhenRegisterUsingMethodAndPropertiesParameters()
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
                        new StateDto { Index = 0, StateTypeName = typeof(int).FullName }
                    },
                    AutowiringTypeName = typeof(SimpleService3).FullName,
                    Methods = new List<IMethodDto>
                    {
                        new MethodDto { Name = nameof(SimpleService3.Init), MethodParameters = new List<IParameterDto> { new ParameterDto { TypeName = typeof(ISimpleService).FullName }}}
                    },
                    Properties = new List<IPropertyDto>
                    {
                        new PropertyDto { Name = nameof(SimpleService3.SimpleService2), Property = new ParameterDto { TypeName = typeof(ISimpleService).FullName, Dependency = new List<IRegisterStatementDto> { new TagDto { Value = "2" } } } }
                    }
                });

            var simpleService = new Mock<ISimpleService>();
            _container.Register().Contract<ISimpleService>().FactoryMethod(ctx => simpleService.Object);

            var simpleService2 = new Mock<ISimpleService>();
            _container.Register().Tag("2").Contract<ISimpleService>().FactoryMethod(ctx => simpleService2.Object);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            configuration.Apply(_container).ToArray();
            var instance = _container.Resolve().Instance<ISimpleService>(33);

            // Then
            instance.ShouldBeOfType<SimpleService3>();
            ((SimpleService3)instance).SimpleService.ShouldBe(simpleService.Object);
            ((SimpleService3)instance).SimpleService2.ShouldBe(simpleService2.Object);
        }

        private ConfigurationDtoAdapter CreateInstance(IConfigurationDto configurationDto)
        {
            var converterStringToObject = new ConverterStringToObject(Reflection.Shared);
            var сonverterValueDtoToObject = new ConverterValueDtoToObject(_typeResolver, converterStringToObject);
            var converterTagDtoToObject = new ConverterTagDtoToObject(_typeResolver, converterStringToObject);
            var converterParameterDtoToParameterMetadata = new ConverterParameterDtoToParameterMetadata(Reflection.Shared, _typeResolver, converterStringToObject, сonverterValueDtoToObject, converterTagDtoToObject);
            var converterRegisterDtoToRegistationResult = new ConverterRegisterDtoToRegistations(Reflection.Shared, _typeResolver, converterTagDtoToObject, converterParameterDtoToParameterMetadata);
            var converterConfigurationDtoToRegistrations = new ConverterConfigurationDtoToRegistrations(converterTagDtoToObject, converterRegisterDtoToRegistationResult);
            var converterConfigurationDtoToDependencies = new ConverterConfigurationDtoToDependencies(_typeResolver);
            return new ConfigurationDtoAdapter(
                configurationDto,
                converterConfigurationDtoToRegistrations,
                converterConfigurationDtoToDependencies);
        }

        private class MyTypeResolver: ITypeResolver
        {
            public List<Assembly> References { get; } = new List<Assembly>();

            public List<string> UsingStatement { get; } = new List<string>();

            public bool TryResolveType(IEnumerable<Assembly> references, IEnumerable<string> usings, string typeName, out Type type)
            {
                References.Clear();
                References.AddRange(references);
                UsingStatement.AddRange(usings);

                if (Strings.IsNullOrWhiteSpace(typeName))
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
            public IEnumerable<IConfiguration> GetDependencies(IContainer container)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IDisposable> Apply(IContainer container)
            {
                throw new NotImplementedException();
            }
        }

        private static class MyFactory
        {
            public static string Create(ICreationContext ctx)
            {
                return ctx.ResolverContext.RegistryContext.Container.Tag?.ToString() ?? "null";
            }

            // ReSharper disable once UnusedParameter.Local
            // ReSharper disable once UnusedMember.Local
            public static string CreateAbcString(ICreationContext ctx)
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

        private class SimpleService2 : ISimpleService, IDisposable
        {
            public SimpleService2(int val)
            {
                Val = val;
            }

            public int Val { get; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class SimpleService3 : ISimpleService, IDisposable
        {
            public void Init([NotNull] ISimpleService simpleService)
            {
                SimpleService = simpleService ?? throw new ArgumentNullException(nameof(simpleService));
            }

            public ISimpleService SimpleService { get; private set; }

            public ISimpleService SimpleService2 { get; private set; }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
