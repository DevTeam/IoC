namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class KeyComparersConfigurationTests
    {
        [Fact]
        public void ShouldResolveWhenAnyTag()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.KeyComparers).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .KeyComparer(Wellknown.KeyComparer.AnyTag)
                    .Tag("abc")
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => mock.Object).Apply())
                {
                    var actualObj = container.Resolve().Tag("xyz").Instance<ISimpleService>();

                    // Then
                    actualObj.ShouldBe(mock.Object);
                }
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}
