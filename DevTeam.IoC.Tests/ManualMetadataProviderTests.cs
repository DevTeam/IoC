// ReSharper disable RedundantUsingDirective
namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class ManualMetadataProviderTests
    {
        private readonly IReflection _reflection = Reflection.Shared;
        private readonly Mock<IMetadataProvider> _defaultMetadataProvider;
        private readonly IParameterMetadata[] _matchedConstructorParams;
        private readonly IParameterMetadata[] _notMatchedByStateTypeConstructorParams;
        private readonly IParameterMetadata[] _notMatchedByContractTypeConstructorParams;
        private readonly CreationContext _creationContext;

        public ManualMetadataProviderTests()
        {
            _defaultMetadataProvider = new Mock<IMetadataProvider>();
            var registryContext = new RegistryContext(Mock.Of<IContainer>(), new[] { Mock.Of<IKey>() }, Mock.Of<IInstanceFactory>());
            var resolverContext = new ResolverContext(Mock.Of<IContainer>(), registryContext, Mock.Of<IInstanceFactory>(), Mock.Of<IKey>());
            _creationContext = new CreationContext(resolverContext, Mock.Of<IStateProvider>());
            _matchedConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 0, typeof(int), true)),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(_reflection, 1, typeof(int), true), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 1, typeof(string), true)),
            };

            _notMatchedByStateTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 0, typeof(double), true)),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(_reflection, 1, typeof(int), true), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 1, typeof(string), true)),
            };

            _notMatchedByContractTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 0, typeof(int), true)),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<double>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(_reflection, 1, typeof(int), true), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 1, typeof(string), true)),
            };
        }

        [Fact]
        public void ShouldUseDefaultMetadataProviderToResolveImplementationType()
        {
            // Given
            var metadataProvider = CreateInstance(Enumerable.Empty<IParameterMetadata>());
            var resolvedType = typeof(IDisposable);
            _defaultMetadataProvider.Setup(i => i.TryResolveType(typeof(IEnumerable<string>), out resolvedType, _creationContext)).Returns(true);

            // When
            var result =  metadataProvider.TryResolveType(typeof(IEnumerable<string>), out resolvedType, _creationContext);

            // Then
            result.ShouldBeTrue();
            resolvedType.ShouldBe(typeof(IDisposable));
        }

        [Fact]
        public void ShouldTrySelectConstructorWhenMatched()
        {
            // Given
            var metadataProvider = CreateInstance(_matchedConstructorParams);

            // When
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out var ctor, out var error);

            // Then
            result.ShouldBeTrue();
            ctor.ShouldBe(typeof(AutowiringClass).GetConstructors().First());
            error.ShouldBeNull();
        }

        [Fact]
        public void ShouldTrySelectConstructorWhenNotMatchedByStateType()
        {
            // Given
            var metadataProvider = CreateInstance(_notMatchedByStateTypeConstructorParams);

            // When
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out _, out _);

            // Then
            result.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTrySelectConstructorWhenNotMatchedByContractType()
        {
            // Given
            var metadataProvider = CreateInstance(_notMatchedByContractTypeConstructorParams);

            // When
            var result = metadataProvider.TrySelectConstructor(typeof(AutowiringClass), out _, out _);

            // Then
            result.ShouldBeFalse();
        }

        [Fact]
        public void ShouldReturnManualCreatedCtorParams()
        {
            // Given
            var metadataProvider = CreateInstance(_matchedConstructorParams);

            // When
            var stateIndex = 0;
            var constructorParameters = metadataProvider.GetParameters(typeof(AutowiringClass).GetConstructors().First(), ref stateIndex);

            // Then
            constructorParameters.ShouldBe(_matchedConstructorParams);
        }


        private IMetadataProvider CreateInstance([NotNull] IEnumerable<IParameterMetadata> ctorParams)
        {
            return new ManualMetadataProvider(_defaultMetadataProvider.Object, _reflection, new TypeMetadata(new MethodMetadata(".ctor", ctorParams), null, null));
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
