namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Shouldly;
    using Xunit;

    public class ChildrenContainersConfigurationTests
    {
        [Fact]
        public void ShouldCreateChildContainer()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.ChildContainers).ToSelf())
            {
                // When
                var childContainer = container.Resolve().Contract<IContainer>().Instance() as IContainer;

                // Then
                childContainer.ShouldNotBeNull();
            }
        }

        [Fact]
        public void ShouldCreateChildContainerWhenHasTag()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.ChildContainers).ToSelf())
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
