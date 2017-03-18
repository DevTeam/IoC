﻿namespace DevTeam.IoC.Tests
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
        private readonly Reflection _reflection = new Reflection();
        private readonly Mock<IMetadataProvider> _defaultMetadataProvider;
        private readonly IParameterMetadata[] _matchedConstructorParams;
        private readonly IParameterMetadata[] _notMatchedByStateTypeConstructorParams;
        private readonly IParameterMetadata[] _notMatchedByContractTypeConstructorParams;
        private readonly Mock<ICreationContext> _creationContext;

        public ManualMetadataProviderTests()
        {
            _defaultMetadataProvider = new Mock<IMetadataProvider>();
            var resolverContext = new Mock<IResolverContext>();
            _creationContext = new Mock<ICreationContext>();
            _creationContext.SetupGet(i => i.ResolverContext).Returns(resolverContext.Object);
            _matchedConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByStateTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(double))),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            _notMatchedByContractTypeConstructorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<double>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<int>), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(1, typeof(int)), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };
        }

        [Fact]
        public void ShouldUseDefaultMetadataProviderToResolveImplementationType()
        {
            // Given
            var metadataProvider = CreateInstance(Enumerable.Empty<IParameterMetadata>());
            Type resolvedType = typeof(IDisposable);
            _defaultMetadataProvider.Setup(i => i.TryResolveImplementationType(_reflection, typeof(IEnumerable<string>), out resolvedType, _creationContext.Object)).Returns(true);

            // When
            var result =  metadataProvider.TryResolveImplementationType(_reflection, typeof(IEnumerable<string>), out resolvedType, _creationContext.Object);

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
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(_reflection, typeof(AutowiringClass), out ctor, out error);

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
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(_reflection, typeof(AutowiringClass), out ctor, out error);

            // Then
            result.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTrySelectConstructorWhenNotMatchedByContractType()
        {
            // Given
            var metadataProvider = CreateInstance(_notMatchedByContractTypeConstructorParams);

            // When
            ConstructorInfo ctor;
            Exception error;
            var result = metadataProvider.TrySelectConstructor(_reflection, typeof(AutowiringClass), out ctor, out error);

            // Then
            result.ShouldBeFalse();
        }

        [Fact]
        public void ShouldReturnManualCreatedCtorParams()
        {
            // Given
            var metadataProvider = CreateInstance(_matchedConstructorParams);

            // When
            var constructorParameters = metadataProvider.GetConstructorParameters(_reflection, typeof(AutowiringClass).GetConstructors().First());

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
