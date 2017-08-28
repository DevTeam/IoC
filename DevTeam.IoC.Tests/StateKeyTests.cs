#if !NET35
namespace DevTeam.IoC.Tests
{
    using System;
    using Shouldly;
    using Xunit;
    using System.Collections.Generic;
    using Contracts;

    public class StateKeyTests
    {
        private readonly IReflection _reflection = Reflection.Shared;

        [Theory]
        [InlineData(0, typeof(string), false, 0, typeof(string), true, true)]
        [InlineData(0, typeof(string), true, 0, typeof(string), false, true)]
        [InlineData(0, typeof(string), false, 1, typeof(string), true, false)]
        [InlineData(0, typeof(string), true, 1, typeof(string), false, false)]

        [InlineData(0, typeof(object), false, 0, typeof(string), true, true)]
        [InlineData(0, typeof(string), false, 0, typeof(object), true, false)]
        [InlineData(0, typeof(IEnumerable<string>), false, 0, typeof(List<string>), true, true)]
        [InlineData(0, typeof(List<string>), false, 0, typeof(IEnumerable<string>), true, false)]
        public void StateKeyShouldImplementEq(int index1, Type stateType1, bool toResolve1, int index2, Type stateType2, bool toResolve2, bool expectedEq)
        {
            // Given
            var key1 = new StateKey(_reflection, index1, stateType1, toResolve1);
            var key2 = new StateKey(_reflection, index2, stateType2, toResolve2);

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
    }
}
#endif
