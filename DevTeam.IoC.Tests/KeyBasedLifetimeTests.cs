namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class KeyBasedLifetimeTests
    {
        private readonly Reflection _reflection = new Reflection();
        private readonly Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private readonly Mock<ILifetime> _baseLifetime;
        private readonly Mock<ILifetimeContext> _lifetimeContext;
        private readonly Mock<IResolverContext> _resolverContext;
        private readonly Mock<ICreationContext> _creationContext;

        public KeyBasedLifetimeTests()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _baseLifetime = new Mock<ILifetime>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _resolverContext = new Mock<IResolverContext>();
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(_resolverContext.Object);
        }

        [Fact]
        public void ShouldSaveInstanceToInternalList()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => 0);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        [Fact]
        public void ShouldClearInternalListWhenDispose()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => 0);
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


        [Fact]
        public void ShouldReturnDifferentObjectsWhenThereAreDifferentKeys()
        {
            // Given
            var key = 0;
            var obj1 = new object();
            var obj2 = new object();
            // ReSharper disable once AccessToModifiedClosure
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            key = 1;
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj1);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            key = 2;
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj2);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object), Times.Exactly(2));
            actualObj1.ShouldBe(obj1);
            actualObj2.ShouldBe(obj2);
            lifetime.Count.ShouldBe(2);
        }

        [Fact]
        public void ShouldReturnSameObjectsWhenThereAreSameKeys()
        {
            // Given
            var key = 0;
            var obj = new object();
            // ReSharper disable once AccessToModifiedClosure
            var lifetime = CreateInstance((lifetimeContext, resolverContext) => key);
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            key = 3;
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            key = 3;
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object), Times.Exactly(2));
            actualObj1.ShouldBe(obj);
            actualObj2.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        private KeyBasedLifetime<int> CreateInstance(Func<ILifetimeContext, ICreationContext, int> keySelector)
        {
            return new MyBasedLifetime(keySelector, _baseLifetime.Object);
        }

        private class MyBasedLifetime : KeyBasedLifetime<int>
        {
            private readonly ILifetime _baseLifetime;

            public MyBasedLifetime(
                Func<ILifetimeContext, ICreationContext, int> keySelector,
                ILifetime baseLifetime)
                : base(keySelector)
            {
                _baseLifetime = baseLifetime;
            }

            protected override ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator)
            {
                return _baseLifetime;
            }
        }
    }
}
