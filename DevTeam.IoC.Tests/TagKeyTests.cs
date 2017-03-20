namespace DevTeam.IoC.Tests
{
    using System;
    using Shouldly;
    using Xunit;
    using System.Collections.Generic;

    public class TagKeyTests
    {
#if !NET35
        private readonly Reflection _reflection = new Reflection();

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
    }

}
