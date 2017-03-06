namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class ManualMetadataProviderTests
    {
        private Mock<IMetadataProvider> _defaultMetadataProvider;
        private Mock<IResolverContext> _resolverContext;
        private IParameterMetadata[] _matchedConstructorParams;
        private IParameterMetadata[] _notMatchedByStateTypeConstructorParams;
        private IParameterMetadata[] _notMatchedByContractTypeConstructorParams;
        private Mock<ICreationContext> _creationContext;

        [SetUp]
        public void SetUp()
        {
            _defaultMetadataProvider = new Mock<IMetadataProvider>();
            _resolverContext = new Mock<IResolverContext>();
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(_resolverContext.Object);
            _matchedConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IContractKey[] {new ContractKey(typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByStateTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(double))),
                new ParameterMetadata(new IContractKey[] {new ContractKey(typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByContractTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IContractKey[] {new ContractKey(typeof(IEnumerable<double>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };
        }

        [Test]
        public void ShouldUseDefaultMetadataProviderToResolveImplementationType()
        {
            // Given
            var metadataProvider = CreateInstance(Enumerable.Empty<IParameterMetadata>());
            Type resolvedType = typeof(IDisposable);
            _defaultMetadataProvider.Setup(i => i.TryResolveImplementationType(typeof(IEnumerable<string>), out resolvedType, _creationContext.Object)).Returns(true);

            // When
            var result =  metadataProvider.TryResolveImplementationType(typeof(IEnumerable<string>), out resolvedType, _creationContext.Object);

            // Then
            result.ShouldBeTrue();
            resolvedType.ShouldBe(typeof(IDisposable));
        }

        [Test]
        public void ShouldTrySelectConstructorWhenMatched()
        {
            // Given
            var metadataProvider = CreateInstance(_matchedConstructorParams);

            // When
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out ctor, out error);

            // Then
            result.ShouldBeTrue();
            ctor.ShouldBe(typeof(AutowiringClass).GetConstructors().First());
            error.ShouldBeNull();
        }

        [Test]
        public void ShouldTrySelectConstructorWhenNotMatchedByStateType()
        {
            // Given
            var metadataProvider = CreateInstance(_notMatchedByStateTypeConstructorParams);

            // When
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out ctor, out error);

            // Then
            result.ShouldBeFalse();
        }

        [Test]
        public void ShouldTrySelectConstructorWhenNotMatchedByContractType()
        {
            // Given
            var metadataProvider = CreateInstance(_notMatchedByContractTypeConstructorParams);

            // When
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out ctor, out error);

            // Then
            result.ShouldBeFalse();
        }

        [Test]
        public void ShouldReturnManualCreatedCtorParams()
        {
            // Given
            var metadataProvider = CreateInstance(_matchedConstructorParams);

            // When
            var constructorParameters = metadataProvider.GetConstructorParameters(typeof(AutowiringClass).GetConstructors().First());

            // Then
            constructorParameters.ShouldBe(_matchedConstructorParams);
        }


        private IMetadataProvider CreateInstance([NotNull] IEnumerable<IParameterMetadata> ctorParams)
        {
            return new ManualMetadataProvider(_defaultMetadataProvider.Object, ctorParams);
        }

        private class AutowiringClass
        {
            public AutowiringClass(
                [State] int arg0,
                IEnumerable<string> arg1,
                [Contract(typeof(IEnumerable<int>))] IEnumerable<int> arg2,
                [Contract(typeof(string))] [State(1, typeof(int))] string arg3,
                [Contract(typeof(string))] [Tag("abc")] string arg4,
                [State] string arg5)
            {
            }
        }
    }
}
