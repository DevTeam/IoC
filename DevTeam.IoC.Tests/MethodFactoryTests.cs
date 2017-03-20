// ReSharper disable UnusedParameter.Local
namespace DevTeam.IoC.Tests
{
    using System.Reflection;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class MethodFactoryTests
    {
        private readonly Mock<IMethodFactory> _instanceFactoryProvider;

        public MethodFactoryTests()
        {
            _instanceFactoryProvider = new Mock<IMethodFactory>();
        }

        [Fact]
        public void ShouldUseInstanceFactoryProviderWhenOverrided()
        {
            // Given
            var simpleService = new SimpleService();
            _instanceFactoryProvider.Setup(i => i.CreateConstructor(It.IsAny<ConstructorInfo>())).Returns(args => simpleService);
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.ChildContainers).ToSelf()
                .CreateChild()
                .Register().Contract<IMethodFactory>().FactoryMethod(ctx => _instanceFactoryProvider.Object).ToSelf()
                .Register().Contract<ISimpleService>().Autowiring<SimpleService>().ToSelf())
            {
                // When
                var actualInstance = container.Resolve().Instance<ISimpleService>();

                // Then
                actualInstance.ShouldBe(simpleService);
            }
        }

        [Fact]
        public void ShouldUseInstanceFactoryProviderWithStateWhenOverrided()
        {
            // Given
            var simpleService = new SimpleService();
            _instanceFactoryProvider.Setup(i => i.CreateConstructor(It.IsAny<ConstructorInfo>())).Returns(a => simpleService);
            using (var container = new Container().Configure()
                .DependsOn(Wellknown.Feature.ChildContainers).ToSelf()
                .CreateChild()
                .Register().Contract<IMethodFactory>().FactoryMethod(ctx => _instanceFactoryProvider.Object).ToSelf()
                .Register().Contract<ISimpleService>().State<string>(0).State<int>(1).Autowiring<SimpleServiceWithState>().ToSelf())
            {
                // When
                var args = new object[] { "abc", 1 };
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
