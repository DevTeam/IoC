namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class AutowiringMetadataProviderTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [TestCase(typeof(string), null, typeof(string))]
        [TestCase(typeof(IEnumerable<>), typeof(IEnumerable<string>), typeof(IEnumerable<string>))]
        [TestCase(typeof(IEnumerable<>), typeof(IList<string>), typeof(IEnumerable<string>))]
        [TestCase(typeof(IEnumerable<string>), typeof(IEnumerable<int>), typeof(IEnumerable<string>))]
        [TestCase(typeof(IEnumerable<>), typeof(IDictionary<int, string>), typeof(IEnumerable<>))]
        public void ShouldResolveImplementationType(Type implementationType, Type contractKeyType, Type expectedResolveImplementationType)
        {
            // Given
            var metadataProvider = CreateInstance();
            var key = new CompositeKey((contractKeyType != null ? Enumerable.Repeat(contractKeyType, 1) : Enumerable.Empty<Type>()).Select(i => new ContractKey(i, true)).Cast<IContractKey>().ToArray(), new ITagKey[0], new IStateKey[0]);
            var resolverContext = new Mock<IResolverContext>();
            resolverContext.SetupGet(i => i.Key).Returns(key);

            // When
            var actualResolveImplementationType = metadataProvider.ResolveImplementationType(resolverContext.Object, implementationType);

            // Then
            actualResolveImplementationType.ShouldBe(expectedResolveImplementationType);
        }

        [Test]
        // Default ctor
        [TestCase(typeof(DefaultCtorClass), true, ".ctor()", null)]
        // One ctor
        [TestCase(typeof(OneCtorClass), true, ".ctor()", null)]
        // One ctor with arg
        [TestCase(typeof(OneCtorClassWithArg), true, ".ctor(System.String)", null)]
        // Сtor with AutowiringAttribute
        [TestCase(typeof(CtorClassWithAutowiringAttribute), true, ".ctor(System.Int32)", null)]
        // Several ctor with AutowiringAttribute
        [TestCase(typeof(SeveralCtorClassWithAutowiringAttribute), false, null, "Too many resolving constructors")]
        // Several ctor
        [TestCase(typeof(SeveralCtorClass), false, null, "Resolving constructor was not found")]
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
                var args = string.Join(", ", ctor.GetParameters().Select(i => i.ParameterType));
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

        [Test]
        public void ShouldGetConstructorParameters()
        {
            // Given
            var metadataProvider = CreateInstance();
            var ctor = typeof(AutowiringClass).GetConstructors().First();
            var expectedCtorParams = new IParameterMetadata[]
            {
                new ParameterMetadata(ctor.DeclaringType, null, 0, new object[0], null, new StateKey(0, typeof(int))),
                new ParameterMetadata(ctor.DeclaringType, new IKey[] { new ContractKey(typeof(IEnumerable<string>), true) },0, new object[0], null, null ),
                new ParameterMetadata(ctor.DeclaringType, new IKey[] { new ContractKey(typeof(IDisposable), true) }, 0, new object[0], null, null ),
                new ParameterMetadata(ctor.DeclaringType, new IKey[] { new ContractKey(typeof(string), true), new StateKey(1, typeof(int)), }, 0, new object[1] { null }, null, null ),
                new ParameterMetadata(ctor.DeclaringType, new IKey[] { new ContractKey(typeof(string), true), new TagKey("abc"), }, 0, new object[0], null, null ),
                new ParameterMetadata(ctor.DeclaringType, null, 0, new object[0], null, new StateKey(1, typeof(string))),
            };

            // When
            var actualCtorParams = metadataProvider.GetConstructorParameters(ctor);

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
            return new AutowiringMetadataProvider();
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
