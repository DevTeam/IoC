#if NETCOREAPP1_0 || NET462 || NET452

namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Shouldly;
    using Xunit;

    public class TuplesTests
    {
        [Fact]
        public void ContainerShouldResolveTuple()
        {
            // Given
            var tuple = (1, str: "abc");
            using (var container = CreateContainer())
            using (container.Configure().ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Tag("a")
                    .Contract<(int, string)>()
                    .FactoryMethod(ctx => tuple))
                {
                    var res = container.Resolve().Tag("a").Instance<(int, string str)>();

                    // Then
                    res.ShouldBe(tuple);
                    res.str.ShouldBe("abc");
                }
            }
        }

        private static IContainer CreateContainer()
        {
            return new Container();
        }
    }
}

#endif