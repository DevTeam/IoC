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
        public void ContanireShouldRegisterAndResolveWhenOneKey()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().AsFactoryMethod(ctx => simpleService.Object))
                {
                    var actualObj = container.Resolve().Contract<ISimpleService>().Instance();

                    // Then
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Test]
        public void ContanireShouldRegisterUsigClassMetadata()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Metadata<ClassWithMetadata>().AsFactoryMethod(ctx => new ClassWithMetadata(ctx.GetState<string>(0))))
                {
                    var actualObj = (ClassWithMetadata)container.Resolve().Contract<ISimpleService>().Tag("abc").State(0, typeof(string)).Instance("xyz");

                    // Then
                    actualObj.Str.ShouldBe("xyz");
                }
            }
        }

        [Test]
        public void ContanireShouldRegisterAndResolveWhenSeveralKeys()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").AsFactoryMethod(ctx => simpleService.Object))
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
        public void ContanireShouldRegisterAndResolveWhenSeveralKeysAndUndefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").AsFactoryMethod(ctx => genericService.Object))
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
        public void ContanireShouldRegisterAndResolveWhenSeveralKeysAndDefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<string>)).Tag("abc").AsFactoryMethod(ctx => genericService.Object))
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = CreateCompositeKey(container, true, new[] { typeof(IGenericService<string>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        public void ContanireShouldRegisterAndResolveWhenSeveralKeysAndSpecifiedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").AsFactoryMethod(ctx => genericService.Object))
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
            public ClassWithMetadata(string str)
            {
                Str = str;
            }

            public string Str { get; private set; }
        }
    }
}