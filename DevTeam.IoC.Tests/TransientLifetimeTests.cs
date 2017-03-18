namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class TransientLifetimeTests
    {
        private readonly Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private readonly Mock<ILifetimeContext> _lifetimeContext;
        private readonly Mock<IResolverFactory> _instanceFactory;
        private readonly Mock<ICreationContext> _creationContext;

        public TransientLifetimeTests()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            var resolverContext = new Mock<IResolverContext>();
            var registryContext = new Mock<IRegistryContext>();
            resolverContext.SetupGet(i => i.RegistryContext).Returns(registryContext.Object);
            _instanceFactory = new Mock<IResolverFactory>();
            registryContext.SetupGet(i => i.InstanceFactory).Returns(_instanceFactory.Object);
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(resolverContext.Object);
        }

        [Fact]
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

        [Fact]
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
