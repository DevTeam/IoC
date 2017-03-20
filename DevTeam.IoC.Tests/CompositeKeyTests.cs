namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class CompositeKeyTests
    {
        private readonly Reflection _refelction = new Reflection();
        private readonly IContractKey _contractKey1;
        private readonly IContractKey _contractKey2;
        private readonly IContractKey _contractKey3;
        private readonly IStateKey _stateKey1;
        private readonly IStateKey _stateKey2;
        private readonly ITagKey _tagKey1;
        private readonly ITagKey _tagKey2;
        private readonly TagKey _tagKey3;

        public CompositeKeyTests()
        {
            _contractKey1 = new ContractKey(_refelction, typeof(string), true);
            _contractKey2 = new ContractKey(_refelction, typeof(IEnumerable<string>), true);
            _contractKey3 = new ContractKey(_refelction, typeof(IEnumerable<>), true);
            _stateKey1 = new StateKey(_refelction, 0, typeof(string), true);
            _stateKey2 = new StateKey(_refelction, 1, typeof(int), true);
            _tagKey1 = new TagKey("abc");
            _tagKey2 = new TagKey(33);
            _tagKey3 = new TagKey("xyz");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void ShouldSupportEqForIdenticalContractAndComposite()
        {
            // Given

            // When
            IKey key1 = CreateInstance(new[] { _contractKey1 });
            IKey key2 = new ContractKey(_refelction, _contractKey1.ContractType, true);

            // Then
            key1.GetHashCode().ShouldBe(key2.GetHashCode());
            key1.Equals(key2).ShouldBeTrue();
            key2.Equals(key1).ShouldBeTrue();
        }

        private CompositeKey CreateInstance(
            [NotNull] IEnumerable<IContractKey> contractKey,
            [CanBeNull] IEnumerable<ITagKey> tagKeys = null,
            [CanBeNull] IEnumerable<IStateKey> stateKeys = null)
        {
            return new CompositeKey(contractKey, tagKeys, stateKeys);
        }
    }
}
