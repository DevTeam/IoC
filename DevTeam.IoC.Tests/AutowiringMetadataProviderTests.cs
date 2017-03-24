namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "EmptyConstructor")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class AutowiringMetadataProviderTests
    {
        private readonly IReflection _reflection = Reflection.Shared;

#if !NET35
        [Theory]
        [InlineData(typeof(string), null, true, typeof(string))]
        [InlineData(typeof(IEnumerable<>), typeof(IEnumerable<string>), true, typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<>), typeof(IList<string>), true, typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<string>), typeof(IEnumerable<int>), true, typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<>), typeof(IDictionary<int, string>), false, null)]
        [InlineData(typeof(IDictionary<,>), typeof(IDictionary<int, string>), true, typeof(IDictionary<int, string>))]
        public void ShouldResolveImplementationType(Type implementationType, Type contractKeyType, bool expectedResolved, Type expectedResolveImplementationType)
        {
            // Given
            var metadataProvider = CreateInstance();
            var key = new CompositeKey((contractKeyType != null ? Enumerable.Repeat(contractKeyType, 1) : Enumerable.Empty<Type>()).Select(i => new ContractKey(_reflection, i, true)).Cast<IContractKey>().ToArray());
            var resolverContext = new Mock<IResolverContext>();
            resolverContext.SetupGet(i => i.Key).Returns(key);
            var creationContext = new Mock<ICreationContext>();
            creationContext.SetupGet(i => i.ResolverContext).Returns(resolverContext.Object);

            // When
            Type actualResolveImplementationType;
            var result = metadataProvider.TryResolveType(implementationType, out actualResolveImplementationType, creationContext.Object);

            // Then
            result.ShouldBe(expectedResolved);
            if(expectedResolved)
            {
                actualResolveImplementationType.ShouldBe(expectedResolveImplementationType);
            }
        }

        [Theory]
        [InlineData(typeof(string), true, typeof(string))]
        [InlineData(typeof(IEnumerable<string>), true, typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<>), false, null)]
        public void ShouldResolveImplementationTypeWhenHasNotCreationContext(
            Type implementationType,
            bool expectedResult,
            Type expectedResolveImplementationType)
        {
            // Given
            var metadataProvider = CreateInstance();

            // When
            Type actualResolveImplementationType;
            var result = metadataProvider.TryResolveType(implementationType, out actualResolveImplementationType);

            // Then
            result.ShouldBe(expectedResult);
            actualResolveImplementationType.ShouldBe(expectedResolveImplementationType);
        }

        [Theory]
        // Default ctor
        [InlineData(typeof(DefaultCtorClass), true, ".ctor()", null)]
        // One ctor
        [InlineData(typeof(OneCtorClass), true, ".ctor()", null)]
        // One ctor with arg
        [InlineData(typeof(OneCtorClassWithArg), true, ".ctor(System.String)", null)]
        // Constructor with AutowiringAttribute
        [InlineData(typeof(CtorClassWithAutowiringAttribute), true, ".ctor(System.Int32)", null)]
        // Several ctor with AutowiringAttribute
        [InlineData(typeof(SeveralCtorClassWithAutowiringAttribute), false, null, "Too many resolving constructors")]
        // Several ctor
        [InlineData(typeof(SeveralCtorClass), false, null, "Resolving constructor was not found")]
        public void ShouldSelectConstructor(Type implementationType, bool expectedResult, string expectedCtorName, string expectedExceptionPattern)
        {
            // Given
            var metadataProvider = CreateInstance();

            // When
            ConstructorInfo ctor;
            Exception exception;
            var actualResult = metadataProvider.TrySelectConstructor(implementationType, out ctor, out exception);

            // Then
            actualResult.ShouldBe(expectedResult);
            if (actualResult)
            {
                var args = string.Join(", ", ctor.GetParameters().Select(i => i.ParameterType.ToString()).ToArray());
                var actualCtorName = $".ctor({args})";
                actualCtorName.ShouldBe(expectedCtorName);
            }
            else
            {
                if (expectedExceptionPattern != null)
                {
                    new Regex(expectedExceptionPattern).Match(exception.Message).Success.ShouldBeTrue();
                }
            }
        }
#endif

        [Fact]
        public void ShouldGetConstructorParameters()
        {
            // Given
            var metadataProvider = CreateInstance();
            var ctor = typeof(AutowiringClass).GetConstructors().First();
            var expectedCtorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 0, typeof(int), true)),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IEnumerable<string>), true)}, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(IDisposable), true) }, null, null, 0, new object[0], null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, null, new IStateKey[] { new StateKey(_reflection, 1, typeof(int), true), }, 0, new object[] { null }, null, null ),
                new ParameterMetadata(new IContractKey[] { new ContractKey(_reflection, typeof(string), true) }, new ITagKey[] { new TagKey("abc") }, null, 0, new object[0], null, null ),
                new ParameterMetadata(null, null, null, 0, new object[0], null, new StateKey(_reflection, 1, typeof(string), true))
            };

            // When
            var stateIndex = 0;
            var actualCtorParams = metadataProvider.GetParameters(ctor, ref stateIndex);

            // Then
            actualCtorParams.Length.ShouldBe(expectedCtorParams.Length);
            actualCtorParams[0].ShouldBe(expectedCtorParams[0]);
            actualCtorParams[1].ShouldBe(expectedCtorParams[1]);
            actualCtorParams[2].ShouldBe(expectedCtorParams[2]);
            actualCtorParams[3].ShouldBe(expectedCtorParams[3]);
            actualCtorParams[4].ShouldBe(expectedCtorParams[4]);
            actualCtorParams[5].ShouldBe(expectedCtorParams[5]);
        }

        private AutowiringMetadataProvider CreateInstance()
        {
            return new AutowiringMetadataProvider(_reflection);
        }

        private class DefaultCtorClass
        {
        }

        private class OneCtorClass
        {
            public OneCtorClass()
            {
            }
        }

        private class OneCtorClassWithArg
        {
            public OneCtorClassWithArg(string str)
            {
            }
        }

        private class CtorClassWithAutowiringAttribute
        {
            public CtorClassWithAutowiringAttribute(string str)
            {
            }

            [Autowiring]
            public CtorClassWithAutowiringAttribute(int num)
            {
            }
        }

        private class SeveralCtorClassWithAutowiringAttribute
        {
            [Autowiring]
            public SeveralCtorClassWithAutowiringAttribute(string str)
            {
            }

            [Autowiring]
            public SeveralCtorClassWithAutowiringAttribute(int num)
            {
            }
        }

        private class SeveralCtorClass
        {
            public SeveralCtorClass(string str)
            {
            }

            public SeveralCtorClass(int num)
            {
            }
        }

        private class AutowiringClass
        {
            public AutowiringClass(
                [State] int arg0,
                IEnumerable<string> arg1,
                [Contract(typeof(IDisposable))] IList<int> arg2,
                [Contract(typeof(string))] [State(1, typeof(int))] string arg3,
                [Contract(typeof(string))] [Tag("abc")] string arg4,
                [State] string arg5)
            {
            }
        }
    }
}
