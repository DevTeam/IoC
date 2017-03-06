namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class TransientLifetimeTests
    {
        private Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private Mock<ILifetimeContext> _lifetimeContext;
        private Mock<IResolverContext> _resolverContext;
        private Mock<IResolverFactory> _instanceFactory;
        private Mock<IRegistryContext> _registryContext;
        private Mock<ICreationContext> _creationContext;

        [SetUp]
        public void SetUp()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _resolverContext = new Mock<IResolverContext>();
            _registryContext = new Mock<IRegistryContext>();
            _resolverContext.SetupGet(i => i.RegistryContext).Returns(_registryContext.Object);
            _instanceFactory = new Mock<IResolverFactory>();
            _registryContext.SetupGet(i => i.InstanceFactory).Returns(_instanceFactory.Object);
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(_resolverContext.Object);
        }

        [Test]
        public void ShouldCreateNewObject()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _instanceFactory.Setup(i => i.Create(_creationContext.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
        }

        [Test]
        public void ShouldCreateNewObjectEachTime()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _instanceFactory.Setup(i => i.Create(_creationContext.Object)).Returns(obj);

            // When
            lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);
            lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);
            lifetime.Create(_lifetimeContext.Object, _creationContext.Object, _lifetimeEnumerator.Object);

            // Then
            _instanceFactory.Verify(i => i.Create(_creationContext.Object), Times.Exactly(3));
        }

        private TransientLifetime CreateInstance()
        {
            return new TransientLifetime();
        }
    }
}
