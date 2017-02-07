namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class AutoDisposingLifetimeTests
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
        public void ShouldUseBaseLifetimeToCreateObjectWhenObjIsNotDisposable()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
            lifetime.Count.ShouldBe(0);
        }

        [Test]
        public void ShouldSaveDisposingToInternalListWhenObjIsDisposable()
        {
            // Given
            var obj = new Mock<IDisposable>();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj.Object);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj.Object);
            lifetime.Count.ShouldBe(1);
        }

        [Test]
        public void ShouldDisposeDisposableWhenDisposing()
        {
            // Given
            var obj = new Mock<IDisposable>();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object)).Returns(obj.Object);
            lifetime.Create(_lifetimeContext.Object, _resolverContext.Object, _lifetimeEnumerator.Object);

            // When
            lifetime.Dispose();

            // Then
            obj.Verify(i => i.Dispose());
        }

        private AutoDisposingLifetime CreateInstance()
        {
            return new AutoDisposingLifetime();
        }
    }
}
