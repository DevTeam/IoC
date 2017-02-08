namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ManualMetadataProviderTests
    {
        private Mock<IMetadataProvider> _defaultMetadataProvider;
        private Mock<IResolverContext> _resolverContext;
        private IParameterMetadata[] _matchedConstructorParams;
        private IParameterMetadata[] _notMatchedByStateTypeConstructorParams;
        private IParameterMetadata[] _notMatchedByContractTypeConstructorParams;

        [SetUp]
        public void SetUp()
        {
            _defaultMetadataProvider = new Mock<IMetadataProvider>();
            _resolverContext = new Mock<IResolverContext>();
            _matchedConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<string>), true) },0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, 0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new TagKey("abc") }, 0, new object[0], null, null ),
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByStateTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(0, typeof(double))),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<string>), true) },0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, 0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new TagKey("abc") }, 0, new object[0], null, null ),
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByContractTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<double>), true) },0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(IEnumerable<int>), true) }, 0, new object[0], null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IKey[] { new ContractKey(typeof(string), true), new TagKey("abc") }, 0, new object[0], null, null ),
                new ParameterMetadata(null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };
        }

        [Test]
        public void ShouldUseDefaultMetadataProviderToResolveImplementationType()
        {
            // Given
            var metadataProvider = CreateInstance(Enumerable.Empty<IParameterMetadata>());
            _defaultMetadataProvider.Setup(i => i.ResolveImplementationType(_resolverContext.Object, typeof(IEnumerable<string>))).Returns(typeof(IDisposable));

            // When
            var resolvedType = metadataProvider.ResolveImplementationType(_resolverContext.Object, typeof(IEnumerable<string>));

            // Then
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
