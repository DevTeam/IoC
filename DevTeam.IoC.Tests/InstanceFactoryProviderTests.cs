// ReSharper disable UnusedParameter.Local
namespace DevTeam.IoC.Tests
{
    using System.Reflection;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class InstanceFactoryProviderTests
    {
        private readonly Mock<IInstanceFactoryProvider> _instanceFactoryProvider;
        private readonly Mock<IInstanceFactory> _instanceFactory;

        public InstanceFactoryProviderTests()
        {
            _instanceFactoryProvider = new Mock<IInstanceFactoryProvider>();
            _instanceFactory = new Mock<IInstanceFactory>();
            _instanceFactoryProvider.Setup(i => i.GetFactory(It.IsAny<ConstructorInfo>())).Returns(_instanceFactory.Object);
        }

        [Fact]
        public void ShouldUseInstanceFactoryProviderWhenOverrided()
        {
            // Given
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.ChildContainers).ToSelf()
                .CreateChild()
                .Register().Contract<IInstanceFactoryProvider>().FactoryMethod(ctx => _instanceFactoryProvider.Object).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                // When
                var simpleService = new SimpleService();
                _instanceFactory.Setup(i => i.Create(It.IsAny<object[]>())).Returns(simpleService);
                var actualInstance = container.Resolve().Instance<ISimpleService>();

                // Then
                actualInstance.ShouldBe(simpleService);
            }
        }

        [Fact]
        public void ShouldUseInstanceFactoryProviderWithStateWhenOverrided()
        {
            // Given
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.ChildContainers).ToSelf()
                .CreateChild()
                .Register().Contract<IInstanceFactoryProvider>().FactoryMethod(ctx => _instanceFactoryProvider.Object).ToSelf()
                .Register().Contract<ISimpleService>().State<string>(0).State<int>(1).Autowiring<SimpleServiceWithState>().ToSelf())
            {
                // When
                var args = new object[] { "abc", 1 };
                var simpleService = new SimpleService();
                _instanceFactory.Setup(i => i.Create(args)).Returns(simpleService);
                var actualInstance = container.Resolve().State<string>(0).State<int>(1).Instance<ISimpleService>(args);

                // Then
                actualInstance.ShouldBe(simpleService);
            }
        }

        private class SimpleService : ISimpleService
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SimpleServiceWithState : ISimpleService
        {
            public SimpleServiceWithState([State] string str, [State] int num)
            {
            }
        }
    }
}
