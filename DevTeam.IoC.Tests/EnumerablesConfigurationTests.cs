namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    public class EnumerablesConfigurationTests
    {
        [Test]
        public void ContanireShouldResolveAllInstancesWhenResolveEnumerable()
        {
            // Given
            var mock1 = new Mock<ISimpleService>();
            var mock2 = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Features.Enumerables).Apply())
            {
                // When
                using (
                    container.Register()
                    .Tag("a")
                    .Contract<ISimpleService>()
                    .AsFactoryMethod(ctx => mock1.Object))
                using (
                    container.Register()
                    .Tag("b")
                    .Contract<ISimpleService>()
                    .AsFactoryMethod(ctx => mock2.Object))
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
        public void ContanireShouldResolveEmptyWhenNoRegistrations()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Features.Enumerables).Apply())
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
