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
        private readonly Mock<IInstanceFactory> _instanceFactory;
        private readonly CreationContext _creationContext;

        public TransientLifetimeTests()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _instanceFactory = new Mock<IInstanceFactory>();
            var registryContext = new RegistryContext(Mock.Of<IContainer>(), new[] { Mock.Of<IKey>() }, _instanceFactory.Object);
            var resolverContext = new ResolverContext(Mock.Of<IContainer>(), registryContext, _instanceFactory.Object, Mock.Of<IKey>());
            _creationContext = new CreationContext(resolverContext, Mock.Of<IStateProvider>());
        }

        [Fact]
        public void ShouldCreateNewObject()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _instanceFactory.Setup(i => i.Create(_creationContext)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
        }

        [Fact]
        public void ShouldCreateNewObjectEachTime()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _instanceFactory.Setup(i => i.Create(_creationContext)).Returns(obj);

            // When
            lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);
            lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);
            lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
            _instanceFactory.Verify(i => i.Create(_creationContext), Times.Exactly(3));
        }

        private TransientLifetime CreateInstance()
        {
            return new TransientLifetime();
        }
    }
}
