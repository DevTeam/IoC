﻿namespace DevTeam.IoC.Tests
{
    using System;
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class ContainerExtensionsTests
    {
        [Fact]
        public void ContainerShouldRegisterAndResolveWhenOneKey()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().FactoryMethod(ctx => simpleService.Object).Apply())
                {
                    var actualObj = container.Resolve().Contract<ISimpleService>().Instance();

                    // Then
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterUsingClassMetadataWhensFactoryMethod()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().FactoryMethod(ctx => new ClassWithMetadata(ctx.GetState<string>(0))).Apply())
                {
                    var actualObj = (ClassWithMetadata)container.Resolve().Contract<ISimpleService>().Tag("abc").State(0, typeof(string)).Instance("xyz");

                    // Then
                    actualObj.Str.ShouldBe("xyz");
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterUsingClassMetadataAutomaticallyWhenAutowiring()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Autowiring<ClassWithMetadata>().Apply())
                {
                    var actualObj = (ClassWithMetadata)container.Resolve().Contract<ISimpleService>().Tag("abc").State(0, typeof(string)).Instance("xyz");

                    // Then
                    actualObj.Str.ShouldBe("xyz");
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeys()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").FactoryMethod(ctx => simpleService.Object).Apply())
                {
                    var actualObj = container.Resolve().Contract<ISimpleService>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = KeyUtils.CreateCompositeKey(container, true, new[] {typeof(ISimpleService)}, new object[]{ "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndUndefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").FactoryMethod(ctx => genericService.Object).Apply())
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = KeyUtils.CreateCompositeKey(container, true, new [] { typeof(IGenericService<>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndDefinedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<string>)).Tag("abc").FactoryMethod(ctx => genericService.Object).Apply())
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = KeyUtils.CreateCompositeKey(container, true, new[] { typeof(IGenericService<string>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        [Fact]
        public void ContainerShouldRegisterAndResolveWhenSeveralKeysAndSpecifiedGenericType()
        {
            // Given
            var genericService = new Mock<IGenericService<string>>();
            using (var container = CreateContainer())
            {
                // When
                using (container.Register().Contract(typeof(IGenericService<>)).Tag("abc").FactoryMethod(ctx => genericService.Object).Apply())
                {
                    var actualObj = container.Resolve().Contract<IGenericService<string>>().Tag("abc").Instance();

                    // Then
                    var resolvingKey = KeyUtils.CreateCompositeKey(container, true, new[] { typeof(IGenericService<>) }, new object[] { "abc" });
                    container.Registrations.ShouldContain(resolvingKey);
                    actualObj.ShouldBe(genericService.Object);
                }
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        // ReSharper disable once UnusedMember.Local
        private class Stub<T>: IMultService<T>
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        [Contract(typeof(ISimpleService))]
        [State(0, typeof(string))]
        [Tag("abc")]
        private class ClassWithMetadata: ISimpleService
        {
            public ClassWithMetadata([State] string str)
            {
                Str = str;
            }

            public string Str { get; }
        }
    }
}