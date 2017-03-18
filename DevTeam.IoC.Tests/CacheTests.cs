namespace DevTeam.IoC.Tests
{
    using Shouldly;
    using Xunit;

    public class CacheTests
    {
        [Fact]
        public void ShouldSetAndGetValue()
        {
            // Given
            var cache = CreateInstance();
            cache.Set(0, "abc");

            // When
            string str;
            var result = cache.TryGet(0, out str);

            // Then
            cache.Count.ShouldBe(1);
            result.ShouldBeTrue();
            str.ShouldBe("abc");
        }

        [Fact]
        public void ShouldRemove()
        {
            // Given
            var cache = CreateInstance();
            cache.Set(0, "abc");
            cache.Set(1, "zyx");

            // When
            var result = cache.TryRemove(1);

            // Then
            cache.Count.ShouldBe(1);
            result.ShouldBeTrue();
            string str;
            cache.TryGet(0, out str).ShouldBeTrue();
        }       

        private Cache<int, string> CreateInstance()
        {
            return new Cache<int, string>();
        }
    }
}
