namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Dto;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ConfigurationFromSringDataTests
    {
        private Container _container;
        private Mock<IConfiguration> _configuration;
        private Mock<IConfiguration> _depConfiguration;
        private Mock<IDisposable> _registration;
        private Mock<IConfigurationDto> _configurationDto;

        [SetUp]
        public void SetUp()
        {
            _container = new Container().Configure().DependsOn(Wellknown.Feature.Dto).ToSelf();
            _configuration = new Mock<IConfiguration>();
            _depConfiguration = new Mock<IConfiguration>();
            _registration = new Mock<IDisposable>();
            _configuration.Setup(i => i.GetDependencies(_container)).Returns(Enumerable.Repeat(_depConfiguration.Object, 1));
            _configuration.Setup(i => i.Apply(_container)).Returns(Enumerable.Repeat(_registration.Object, 1));
            _configurationDto = new Mock<IConfigurationDto>();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void ShouldCeateRegistrationWhenItHasIConfigurationDescriptionDtoAsState()
        {
            // Given

            var instance = CreateInstance();
            IResolverContext context = null;
            _container.Register()
                .Tag(typeof(MyConfig))
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfiguration>().FactoryMethod(ctx =>
                {
                    context = ctx;
                    return _configuration.Object;
                })
                .ToSelf();

            // When
            var actualDependencies = instance.GetDependencies(_container).ToList();
            var actualRegistrations = instance.Apply(_container).ToList();

            // Then
            _configuration.Verify(i => i.GetDependencies(_container), Times.Once);
            _configuration.Verify(i => i.Apply(_container), Times.Once);
            actualDependencies.ShouldBe(Enumerable.Repeat(_depConfiguration.Object, 1));
            actualRegistrations.ShouldBe(Enumerable.Repeat(_registration.Object, 1));
            context.Container.ShouldBe(_container);
        }

        [Test]
        public void ShouldCeateRegistrationWhenItHasIConfigurationDtoAsState()
        {
            // Given
            var instance = CreateInstance();
            IResolverContext context = null;
            _container.Register()
                .Tag(typeof(MyConfig))
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfigurationDto>().FactoryMethod(ctx =>
                {
                    context = ctx;
                    return _configurationDto.Object;
                })
                .ToSelf();

            // When

            // Then
            instance.BaseConfiguration.ShouldBeOfType<ConfigurationDtoAdapter>();
            context.Container.ShouldBe(_container);
        }

        private ConfigurationFromSringData CreateInstance()
        {
            return new ConfigurationFromSringData(_container, typeof(MyConfig), "data");
        }

        private class MyConfig : IConfiguration
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
    }
}
