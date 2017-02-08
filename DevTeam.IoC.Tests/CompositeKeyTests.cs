namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class CompositeKeyTests
    {
        private IContractKey _contractKey1;
        private IContractKey _contractKey2;
        private IContractKey _contractKey3;
        private IStateKey _stateKey1;
        private IStateKey _stateKey2;
        private ITagKey _tagKey1;
        private ITagKey _tagKey2;
        private TagKey _tagKey3;

        [SetUp]
        public void SetUp()
        {
            _contractKey1 = new ContractKey(typeof(string), true);
            _contractKey2 = new ContractKey(typeof(IEnumerable<string>), true);
            _contractKey3 = new ContractKey(typeof(IEnumerable<>), true);
            _stateKey1 = new StateKey(0, typeof(string));
            _stateKey2 = new StateKey(1, typeof(int));
            _tagKey1 = new TagKey("abc");
            _tagKey2 = new TagKey(33);
            _tagKey3 = new TagKey("xyz");
        }

        [Test]
        public void ShouldSupportEqForIdentical()
        {
            // Given

            // When
            var key1 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2, _tagKey3 }, new[] { _stateKey1, _stateKey2 });
            var key2 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2, _tagKey3 }, new[] { _stateKey1, _stateKey2 });

            // Then
            key1.GetHashCode().ShouldBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeTrue();
        }

        [Test]
        public void ShouldSupportEqForReordered()
        {
            // Given

            // When
            var key1 = CreateInstance(new[] { _contractKey2, _contractKey1, _contractKey3 }, new[] { _tagKey3, _tagKey2, _tagKey1 }, new[] { _stateKey2, _stateKey1 });
            var key2 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey3, _tagKey2 }, new[] { _stateKey1, _stateKey2 });

            // Then
            key1.GetHashCode().ShouldBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeTrue();
        }

        [Test]
        public void ShouldSupportEqForNotIdenticalByContract()
        {
            // Given

            // When
            var key1 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2 }, new[] { _stateKey1, _stateKey2 });
            var key2 = CreateInstance(new[] { _contractKey1, _contractKey3 }, new[] { _tagKey1, _tagKey2 }, new[] { _stateKey1, _stateKey2 });

            // Then
            key1.GetHashCode().ShouldNotBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeFalse();
        }

        [Test]
        public void ShouldSupportEqForNotIdenticalByTag()
        {
            // Given

            // When
            var key1 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2 }, new[] { _stateKey1, _stateKey2 });
            var key2 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey2 }, new[] { _stateKey1, _stateKey2 });

            // Then
            key1.GetHashCode().ShouldNotBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeFalse();
        }

        [Test]
        public void ShouldSupportEqForNotIdenticalByState()
        {
            // Given

            // When
            var key1 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2 }, new[] { _stateKey1, _stateKey2 });
            var key2 = CreateInstance(new[] { _contractKey1, _contractKey2, _contractKey3 }, new[] { _tagKey1, _tagKey2 }, new[] { _stateKey1 });

            // Then
            key1.GetHashCode().ShouldNotBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeFalse();
        }

        private CompositeKey CreateInstance(
            [NotNull] IContractKey[] contractKey,
            [NotNull] ITagKey[] tagKeys,
            [NotNull] IStateKey[] stateKeys)
        {
            return new CompositeKey(contractKey, tagKeys, stateKeys);
        }
    }
}
