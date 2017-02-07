namespace DevTeam.IoC.Tests
{
    using System;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class AutowiringInstanceFactoryTests
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [Test]
        public void ShouldCreateObjectWhenDefaultCtor()
        {
            // Given
            var factory = CreateFactory<SimpleClass>();

            // When
            var instance = factory.Create(CreateContext());

            // Then
            instance.ShouldBeOfType<SimpleClass>();
        }

        [Test]
        public void ShouldCreateObjectWhenDefaultCtorAndStaticInitializer()
        {
            // Given
            var factory = CreateFactory<SimpleClassWithStatitcInitializer>();

            // When
            var instance = factory.Create(CreateContext());

            // Then
            instance.ShouldBeOfType<SimpleClassWithStatitcInitializer>();
        }

        [Test]
        public void ShouldCreateObjectWhenStateClass()
        {
            // Given
            var factory = CreateFactory<StateClass>();

            // When
            var instance = (StateClass)factory.Create(CreateContext("abc", 3));

            // Then
            instance.Arg1.ShouldBe("abc");
            instance.Arg2.ShouldBe(3);
        }

        [Test]
        public void ShouldCreateObjectWhenDependencyClass()
        {
            // Given
            var factory = CreateFactory<DependencyClass>();
            _container.Register().Contract<StateClass>().Tag("xxx").State(1, typeof(int)).State(0, typeof(string)).AsFactoryMethod(ctx => new StateClass("aa", 4));

            // When
            var instance = (DependencyClass)factory.Create(CreateContext("abc", 3));

            // Then
            instance.Arg1.ShouldBe("abc");
            instance.Arg2.ShouldBe(3);
            instance.StateClass.ShouldBeOfType<StateClass>();
            instance.StateClass.ShouldNotBeNull();
        }

        [Test]
        public void ShouldCreateObjectWhenDependencyWithStateClass()
        {
            // Given
            var factory = CreateFactory<DependencyWithVauesClass>();
            _container.Register().Contract<StateClass>().Tag("xyz").State(0, typeof(string)).State(1, typeof(int)).AsAutowiring<StateClass>();

            // When
            var instance = (DependencyWithVauesClass)factory.Create(CreateContext("abc", 3));

            // Then
            instance.Arg1.ShouldBe("abc");
            instance.Arg2.ShouldBe(3);
            instance.StateClass.ShouldBeOfType<StateClass>();
            instance.StateClass.Arg1.ShouldBe("fgh");
            instance.StateClass.Arg2.ShouldBe(5);
        }

        private static IResolverFactory CreateFactory<T>()
        {
            return new MetadataFactory(typeof(T), new ExpressionInstanceFactoryProvider(), new AutowiringMetadataProvider());
        }

        private IResolverContext CreateContext(params object[] state)
        {
            var registryContext = new Mock<IRegistryContext>();
            registryContext.SetupGet(i => i.Container).Returns(_container);
            var resolverContext = new Mock<IResolverContext>();
            resolverContext.SetupGet(i => i.RegistryContext).Returns(registryContext.Object);
            resolverContext.SetupGet(i => i.StateProvider).Returns(new ParamsStateProvider(state));
            return resolverContext.Object;
        }

        private class SimpleClass
        {
        }

        private class SimpleClassWithStatitcInitializer
        {
#pragma warning disable 169
            public static readonly int Num = 99;
#pragma warning restore 169
        }

        private class StateClass
        {
            public StateClass([State] string arg1, [State] int arg2)
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public int Arg2 { get; }
        }

        private class DependencyClass
        {
            public DependencyClass(
                [State] string arg1,
                [Contract(typeof(StateClass))] [Tag("xxx")] [State(0, typeof(string))] [State(1, typeof(int))] StateClass stateClass,
                [State] int arg2)
            {
                Arg1 = arg1;
                StateClass = stateClass;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public StateClass StateClass { get; }

            public int Arg2 { get; }
        }

        private class DependencyWithVauesClass
        {
            public DependencyWithVauesClass(
                [State] string arg1,
                [Contract(typeof(StateClass))] [Tag("xyz")] [State(1, typeof(int), Value = 5)] [State(0, typeof(string), Value = "fgh")] StateClass stateClass,
                [State] int arg2)
            {
                Arg1 = arg1;
                StateClass = stateClass;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public StateClass StateClass { get; }

            public int Arg2 { get; }
        }
    }
}
