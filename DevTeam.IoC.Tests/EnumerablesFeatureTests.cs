namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class EnumerablesFeatureTests
    {
        [Fact]
        public void ShouldSupportResolvingEnumerable()
        {
            // Given
            var simpleService1 = new Mock<ISimpleService>();
            var simpleService2 = new Mock<ISimpleService>();
            var simpleService3 = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Enumerables).Apply())
            {
                // When
                using (container.Register().Contract<ISimpleService>().FactoryMethod(ctx => simpleService1.Object))
                using (container.Register().Tag(1).Contract<ISimpleService>().FactoryMethod(ctx => simpleService2.Object))
                using (container.Register().Tag("a").Contract<ISimpleService>().FactoryMethod(ctx => simpleService3.Object))
                {
                    var actualList = container.Resolve().Instance<IEnumerable<ISimpleService>>().ToList();

                    // Then
                    actualList.Count.ShouldBe(3);
                    actualList.ShouldBe(new []{simpleService1.Object, simpleService2.Object , simpleService3.Object });
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveAllInstancesWhenResolveEnumerable()
        {
            // Given
            var mock1 = new Mock<ISimpleService>();
            var mock2 = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Enumerables).ToSelf())
            {
                // When
                using (
                    container.Register()
                        .Tag("a")
                        .Contract<ISimpleService>()
                        .FactoryMethod(ctx => mock1.Object))
                using (
                    container.Register()
                        .Tag("b")
                        .Contract<ISimpleService>()
                        .FactoryMethod(ctx => mock2.Object))
                {
                    var listOfObj = container.Resolve().Instance<IEnumerable<ISimpleService>>().ToList();

                    // Then
                    listOfObj.Count.ShouldBe(2);
                    listOfObj.ShouldContain(mock1.Object);
                    listOfObj.ShouldContain(mock2.Object);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveEmptyWhenNoRegistrations()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Enumerables).ToSelf())
            {
                // When
                var listOfObj = container.Resolve().Instance<IEnumerable<ISimpleService>>().ToList();

                // Then
                listOfObj.Count.ShouldBe(0);
            }
        }

        private static IContainer CreateContainer()
        {
            return new Container();
        }
    }
}