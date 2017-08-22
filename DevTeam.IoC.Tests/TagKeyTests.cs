namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class TagKeyTests
    {
#if !NET35
        [Theory]
        [InlineData(3, 3, true)]
        [InlineData(1, 3, false)]
        [InlineData(3, "3", false)]
        public void TagKeyShouldImplementEq(object value1, object value2, bool expectedEq)
        {
            // Given
            var key1 = new TagKey(value1);
            var key2 = new TagKey(value2);

            // When
            var hashCode1 = key1.GetHashCode();
            var hashCode2 = key2.GetHashCode();
            var actualEq1 = Equals(key1, key2);
            var actualEq2 = Equals(key2, key1);

            // Then
            if (expectedEq)
            {
                hashCode1.ShouldBe(hashCode2);
            }

            actualEq1.ShouldBe(expectedEq);
            actualEq2.ShouldBe(expectedEq);
        }
#endif

        [Fact]
        public void ContainerShouldRegisterAndResolveWhenOneKey()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                // When
                using (container.Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Tag(1, 2, 3)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => simpleService.Object))
                {
                    var actualObj1 = container.Resolve().Tag(1).Instance<ISimpleService>();
                    var actualObj2 = container.Resolve().Tag(2).Instance<ISimpleService>();
                    var actualObj3 = container.Resolve().Tag(2, 1).Instance<ISimpleService>();
                    var actualObj4 = container.Resolve().Tag(2, 3, 1).Instance<ISimpleService>();
                    var actualObj5 = container.Resolve().Tag(1, 2, 3).Instance<ISimpleService>();
                    var res6 = container.Resolve().Tag(1, 2, 3, 4).TryInstance(out ISimpleService _);
                    var res7 = container.Resolve().TryInstance(out ISimpleService _);
                    var res8 = container.Resolve().Tag(4).TryInstance(out ISimpleService _);

                    // Then
                    actualObj1.ShouldBe(simpleService.Object);
                    actualObj2.ShouldBe(actualObj1);
                    actualObj3.ShouldBe(actualObj1);
                    actualObj4.ShouldBe(actualObj1);
                    actualObj5.ShouldBe(actualObj1);
                    res6.ShouldBeTrue();
                    res7.ShouldBeFalse();
                    res8.ShouldBeFalse();
                }
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}
