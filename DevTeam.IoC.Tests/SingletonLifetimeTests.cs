namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class SingletonLifetimeTests
    {
        private readonly Reflection _reflection = new Reflection();
        private Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private Mock<ILifetime> _baseLifetime;
        private Mock<ILifetimeContext> _lifetimeContext;
        private Mock<IResolverContext> _resolverContext;
        private Mock<ICreationContext> _creationContext;

        [SetUp]
        public void SetUp()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _baseLifetime = new Mock<ILifetime>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _resolverContext = new Mock<IResolverContext>();
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(_resolverContext.Object);
        }

        [Test]
        public void ShouldSaveInstanceToInternalList()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[]{ new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        [Test]
        public void ShouldClearInternalListWhenDispose()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);
            lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // When
            lifetime.Dispose();

            // Then
            lifetime.Count.ShouldBe(0);
        }

        [Test]
        public void ShouldReturnTheSameObjectWhenTheSameGenericArguments()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object), Times.Exactly(1));
            actualObj1.ShouldBe(obj);
            actualObj1.ShouldBe(actualObj2);
            lifetime.Count.ShouldBe(1);
        }

        [Test]
        public void ShouldReturnTheSameObjectWhenDifferentTags()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            var key1 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key1);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            var key2 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("xyz") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key2);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object), Times.Exactly(1));
            actualObj1.ShouldBe(actualObj2);
            lifetime.Count.ShouldBe(1);
        }


        [Test]
        public void ShouldReturnDifferentObjectsWhenThereAreDifferentGenericArguments()
        {
            // Given
            var obj1 = new object();
            var obj2 = new object();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            var key1 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key1);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj1);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            var key2 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key2);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj2);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object), Times.Exactly(2));
            actualObj1.ShouldBe(obj1);
            actualObj2.ShouldBe(obj2);
            lifetime.Count.ShouldBe(2);
        }

        private SingletonLifetime CreateInstance()
        {
            return new SingletonLifetime();
        }
    }
}
