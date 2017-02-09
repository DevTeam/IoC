namespace DevTeam.IoC.Tests
{
    using System;
    using System.Linq;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ContainerExtensionsTests
    {
        [Test]
        public void ContainerShouldRegisterAndResolveWhenOneKey()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().FactoryMethod(ctx => simpleService.Object))
                {
                    var actualObj = container.Resolve().Contract<ISimpleService>().Instance();

                    // Then
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterUsigClassMetadataWhensFactoryMethod()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().FactoryMethod(ctx => new ClassWithMetadata(ctx.GetState<string>(0))))
                {
                    var actualObj = (ClassWithMetadata)container.Resolve().Contract<ISimpleService>().Tag("abc").State(0, typeof(string)).Instance("xyz");

                    // Then
                    actualObj.Str.ShouldBe("xyz");
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterUsigClassMetadataAutomaticallyWhenAutowiring()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Autowiring<ClassWithMetadata>())
                {
                    var actualObj = (ClassWithMetadata)container.Resolve().Contract<ISimpleService>().Tag("abc").State(0, typeof(string)).Instance("xyz");

                    // Then
                    actualObj.Str.ShouldBe("xyz");
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeys()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").FactoryMethod(ctx => simpleService.Object))
                {
                    var actualObj = container.Resolve().Contract<ISimpleService>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = CreateCompositeKey(container, true, new[] {typeof(ISimpleService)}, new object[]{ "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndUndefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").FactoryMethod(ctx => genericService.Object))
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = CreateCompositeKey(container, true, new [] { typeof(IGenericService<>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndDefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<string>)).Tag("abc").FactoryMethod(ctx => genericService.Object))
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = CreateCompositeKey(container, true, new[] { typeof(IGenericService<string>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndSpecifiedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").FactoryMethod(ctx => genericService.Object))
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = CreateCompositeKey(container, true, new[] { typeof(IGenericService<>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        private ICompositeKey CreateCompositeKey(IContainer container, bool toResolve, Type[] genericTypes, object[] tags)
        {
            var keyFactory = container.GetKeyFactory();
            var genericKeys = genericTypes.Select(i => keyFactory.CreateContractKey(i, toResolve)).ToArray();
            var tagKeys = tags.Select(i => keyFactory.CreateTagKey(i)).ToArray();
            var stateKeys = new IStateKey[0];
            return keyFactory.CreateCompositeKey(
                genericKeys,
                tagKeys,
                stateKeys);
        }

        private class Stub<T>: IMultService<T>
        {
            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        [Contract(typeof(ISimpleService))]
        [State(0, typeof(string))]
        [Tag("abc")]
        public class ClassWithMetadata: ISimpleService
        {
            public ClassWithMetadata([State] string str)
            {
                Str = str;
            }

            public string Str { get; private set; }
        }
    }
}