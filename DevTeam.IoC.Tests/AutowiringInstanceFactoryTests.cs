﻿namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Moq;
    using Shouldly;
    using Xunit;

    public class AutowiringInstanceFactoryTests
    {
        private readonly IContainer _container;

        public AutowiringInstanceFactoryTests()
        {
            _container = new Container();
        }

        
        [Fact]
        public void ShouldCreateObjectWhenDefaultCtor()
        {
            // Given
            var factory = CreateFactory<SimpleClass>();

            // When
            var instance = factory.Create(CreateContext());

            // Then
            instance.ShouldBeOfType<SimpleClass>();
        }

        [Fact]
        public void ShouldCreateObjectWhenDefaultCtorAndStaticInitializer()
        {
            // Given
            var factory = CreateFactory<SimpleClassWithStaticInitializer>();

            // When
            var instance = factory.Create(CreateContext());

            // Then
            instance.ShouldBeOfType<SimpleClassWithStaticInitializer>();
        }

        [Fact]
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

        [Fact]
        public void ShouldCreateObjectWhenDependencyClass()
        {
            // Given
            var factory = CreateFactory<DependencyClass>();
            _container.Register().Contract<StateClass>().Tag("xxx").State(1, typeof(int)).State(0, typeof(string)).FactoryMethod(ctx => new StateClass("aa", 4));

            // When
            var instance = (DependencyClass)factory.Create(CreateContext("abc", 3));

            // Then
            instance.Arg1.ShouldBe("abc");
            instance.Arg2.ShouldBe(3);
            instance.StateClass.ShouldBeOfType<StateClass>();
            instance.StateClass.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldCreateObjectWhenDependencyWithStateClass()
        {
            // Given
            var factory = CreateFactory<DependencyWithValuesClass>();
            _container.Register().Contract<StateClass>().Tag("xyz").State(0, typeof(string)).State(1, typeof(int)).Autowiring<StateClass>();

            // When
            var instance = (DependencyWithValuesClass)factory.Create(CreateContext("abc", 3));

            // Then
            instance.Arg1.ShouldBe("abc");
            instance.Arg2.ShouldBe(3);
            instance.StateClass.ShouldBeOfType<StateClass>();
            instance.StateClass.Arg1.ShouldBe("fgh");
            instance.StateClass.Arg2.ShouldBe(5);
        }

        private IInstanceFactory CreateFactory<T>()
        {
            return new MetadataFactory(typeof(T), new ExpressionMethodFactory(), new AutowiringMetadataProvider(Reflection.Shared), _container.KeyFactory);
        }

        private CreationContext CreateContext(params object[] state)
        {
            var registryContext = new RegistryContext(_container, new[] { Mock.Of<IKey>() }, Mock.Of<IInstanceFactory>());
            var resolverContext = new ResolverContext(_container, registryContext, Mock.Of<IInstanceFactory>(), Mock.Of<IKey>());
            var creationContext = new CreationContext(resolverContext, ParamsStateProvider.Create(state));
            return creationContext;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SimpleClass
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SimpleClassWithStaticInitializer
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

        // ReSharper disable once ClassNeverInstantiated.Local
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

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DependencyWithValuesClass
        {
            public DependencyWithValuesClass(
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
