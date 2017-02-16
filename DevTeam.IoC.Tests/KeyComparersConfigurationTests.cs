namespace DevTeam.IoC.Tests
{
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    public class KeyComparersConfigurationTests
    {
        [Test]
        public void ShouldResolveWhenAnyTag()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.KeyComaprers).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .KeyComparer(Wellknown.KeyComparer.AnyTag)
                    .Tag("abc")
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => mock.Object).Create())
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
