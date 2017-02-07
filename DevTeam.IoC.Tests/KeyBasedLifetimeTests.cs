namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class KeyBasedLifetimeTests
    {
        private Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private Mock<ILifetime> _baseLifetime;
        private Mock<ILifetimeContext> _lifetimeContext;
        private Mock<IResolverContext> _resolverContext;

        [SetUp]
        public void SetUp()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _baseLifetime = new Mock<ILifetime>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _resolverContext = new Mock<IResolverContext>();
        }

        [Test]
        public void ShouldSaveInstanceToInternalList()
        {
            // Given
            var obj = new object();
            var registrationKey = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => 0);
            _resolverContext.SetupGet(i => i.RegistrationKey).Returns(registrationKey);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        [Test]
        public void ShouldClearInternalListWhenDispose()
        {
            // Given
            var obj = new object();
            var registrationKey = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => 0);
            _resolverContext.SetupGet(i => i.RegistrationKey).Returns(registrationKey);
            var key = new CompositeKey(new IContractKey[] { new ContractKey(typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(0, typeof(string)) });
            _resolverContext.SetupGet(i => i.Key).Returns(key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj);
            lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // When
            lifetime.Dispose();

            // Then
            lifetime.Count.ShouldBe(0);
        }


        [Test]
        public void ShouldReturnDifferentObjectsWhenThereAreDifferentKeys()
        {
            // Given
            var key = 0;
            var obj1 = new object();
            var obj2 = new object();
            var registrationKey = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => key);
            _resolverContext.SetupGet(i => i.RegistrationKey).Returns(registrationKey);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            key = 1;
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj1);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            key = 2;
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj2);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object), Times.Exactly(2));
            actualObj1.ShouldBe(obj1);
            actualObj2.ShouldBe(obj2);
            lifetime.Count.ShouldBe(2);
        }

        [Test]
        public void ShouldReturnSameObjectsWhenThereAreSameKeys()
        {
            // Given
            var key = 0;
            var obj = new object();
            var registrationKey = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => key);
            _resolverContext.SetupGet(i => i.RegistrationKey).Returns(registrationKey);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            key = 3;
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            key = 3;
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object), Times.Exactly(2));
            actualObj1.ShouldBe(obj);
            actualObj2.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        private KeyBasedLifetime<int> CreateInstance(Func<ILifetimeContext, IResolverContext, int> keySelector)
        {
            return new KeyBasedLifetime<int>(keySelector, () => _baseLifetime.Object);
        }
    }
}
