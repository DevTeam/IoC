namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    public class EnumConfigurationTests
    {
        [Test]
        public void ContainerShouldResolveAllInstancesWhenResolveEnumerable()
        {
            // Given
            var mock1 = new Mock<ISimpleService>();
            var mock2 = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Enumerables).Own())
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

        [Test]
        public void ContainerShouldResolveEmptyWhenNoRegistrations()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Enumerables).Own())
            {
                // When
                var listOfObj = container.Resolve().Instance<IEnumerable<ISimpleService>>().ToList();

                // Then
                listOfObj.Count.ShouldBe(0);
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}
