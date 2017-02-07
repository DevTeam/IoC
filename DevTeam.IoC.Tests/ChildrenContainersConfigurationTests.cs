namespace DevTeam.IoC.Tests
{
    using Contracts;

    using NUnit.Framework;

    using Shouldly;

    public class ChildrenContainersConfigurationTests
    {
        [Test]
        public void ShouldCreateChildContainer()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Features.ChildContainers).Apply())
            {
                // When
                var childContainer = container.Resolve().Contract<IContainer>().Instance() as IContainer;

                // Then
                childContainer.ShouldNotBeNull();
            }
        }

        [Test]
        public void ShouldCreateChildContainerWhenHasTag()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Features.ChildContainers).Apply())
            {
                // When
                var childContainer = container.Resolve().Contract<IContainer>().State(0, typeof(object)).Instance("abc") as IContainer;

                // Then
                childContainer.ShouldNotBeNull();
                // ReSharper disable once PossibleNullReferenceException
                childContainer.Tag.ShouldBe("abc");
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}
