namespace DevTeam.IoC.Tests
{
    using System;
    using NUnit.Framework;
    using Shouldly;

    public class ContractKeyTests
    {
        private readonly Reflection _reflection = new Reflection();

        [Test]
        [TestCase(typeof(string), false, typeof(string), true, true)]
        [TestCase(typeof(IEquatable<string>), false, typeof(IEquatable<string>), true, true)]
        [TestCase(typeof(IEquatable<>), false, typeof(IEquatable<string>), true, true)]
        [TestCase(typeof(IEquatable<string>), false, typeof(IEquatable<>), true, false)]
        [TestCase(typeof(IEquatable<int>), false, typeof(IEquatable<string>), true, false)]
        [TestCase(typeof(string), true, typeof(string), true, true)]
        [TestCase(typeof(string), true, typeof(int), true, false)]
        [TestCase(typeof(string), false, typeof(string), false, true)]
        [TestCase(typeof(string), false, typeof(int), false, false)]
        public void ContractKeyShouldImplementEq(Type contractType1, bool toResolve1, Type contractType2, bool toResolve2, bool expectedEq)
        {
            // Given
            var key1 = new ContractKey(_reflection, contractType1, toResolve1);
            var key2 = new ContractKey(_reflection, contractType2, toResolve2);

            // When
            var hashCode1 = key1.GetHashCode();
            var hashCode2 = key1.GetHashCode();
            var actualEq1 = Equals(key1, key2);
            var actualEq2 = Equals(key2, key1);

            // Then
            hashCode1.ShouldBe(hashCode2);
            actualEq1.ShouldBe(expectedEq);
            actualEq2.ShouldBe(expectedEq);
        }
    }
}
