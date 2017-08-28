namespace DevTeam.IoC.Tests
{
    using System.Collections.Generic;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class SingletonLifetimeTests
    {
        private readonly IReflection _reflection = Reflection.Shared;
        private readonly Mock<IEnumerator<ILifetime>> _lifetimeEnumerator;
        private readonly Mock<ILifetime> _baseLifetime;
        private readonly Mock<ILifetimeContext> _lifetimeContext;
        private readonly RegistryContext _registryContext;
        private ResolverContext _resolverContext;
        private CreationContext _creationContext;

        public SingletonLifetimeTests()
        {
            _lifetimeEnumerator = new Mock<IEnumerator<ILifetime>>();
            _baseLifetime = new Mock<ILifetime>();
            _lifetimeContext = new Mock<ILifetimeContext>();
            _registryContext = new RegistryContext(Mock.Of<IContainer>(), new[] { Mock.Of<IKey>() }, Mock.Of<IInstanceFactory>());
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), Mock.Of<IKey>());
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
        }

        [Fact]
        public void ShouldSaveInstanceToInternalList()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[]{ new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
            actualObj.ShouldBe(obj);
            lifetime.Count.ShouldBe(1);
        }

        [Fact]
        public void ShouldClearInternalListWhenDispose()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj);
            lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // When
            lifetime.Dispose();

            // Then
            lifetime.Count.ShouldBe(0);
        }

        [Fact]
        public void ShouldReturnTheSameObjectWhenTheSameGenericArguments()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            var key = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj);

            // When
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
            _baseLifetime.Verify(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object), Times.Exactly(1));
            actualObj1.ShouldBe(obj);
            actualObj1.ShouldBe(actualObj2);
            lifetime.Count.ShouldBe(1);
        }

        [Fact]
        public void ShouldReturnSameObjectWhenDifferentTags()
        {
            // Given
            var obj = new object();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            var key1 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key1);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            var key2 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("xyz") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key2);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
            actualObj1.ShouldBe(actualObj2);
            lifetime.Count.ShouldBe(1);
        }

        [Fact]
        public void ShouldReturnDifferentObjectsWhenThereAreDifferentGenericArguments()
        {
            // Given
            var obj1 = new object();
            var obj2 = new object();
            var lifetime = CreateInstance();
            _lifetimeEnumerator.Setup(i => i.MoveNext()).Returns(true);
            _lifetimeEnumerator.SetupGet(i => i.Current).Returns(_baseLifetime.Object);

            // When
            var key1 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key1);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj1);
            var actualObj1 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            var key2 = new CompositeKey(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, new ITagKey[] { new TagKey("abc") }, new IStateKey[] { new StateKey(_reflection, 0, typeof(string), true) });
            _resolverContext = new ResolverContext(Mock.Of<IContainer>(), _registryContext, Mock.Of<IInstanceFactory>(), key2);
            _creationContext = new CreationContext(_resolverContext, Mock.Of<IStateProvider>());
            _baseLifetime.Setup(i => i.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object)).Returns(obj2);
            var actualObj2 = lifetime.Create(_lifetimeContext.Object, _creationContext, _lifetimeEnumerator.Object);

            // Then
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
