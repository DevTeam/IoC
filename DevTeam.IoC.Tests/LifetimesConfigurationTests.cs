namespace DevTeam.IoC.Tests
{
    using System.Threading;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    public class LifetimesConfigurationTests
    {
        [Test]
        public void ContainerShouldResolveSingleInstanceWhenSingletone()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => new Mock<ISimpleService>().Object).Apply())
                {
                    var actualObj1 = container.Resolve().Instance<ISimpleService>();
                    var actualObj2 = container.Resolve().Instance<ISimpleService>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                }
            }
        }

        [Test]
        public void ContainerShouldResolveSingleInstanceWhenSingletoneUsingDifferentContracts()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Contract<ISimpleService>()
                    .Contract<IDisposableService>()
                    .FactoryMethod(ctx => new Stub()).Apply())
                {
                    object actualObj1 = container.Resolve().Instance<ISimpleService>();
                    object actualObj2 = container.Resolve().Instance<IDisposableService>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                }
            }
        }

        [Test]
        public void ContainerShouldResolveDifInstancesWhenDifStateAndPerState()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.PerState)
                    .Contract<Statefull>()
                    .State<string>(0)
                    .FactoryMethod(ctx => new Statefull(ctx.GetState<string>(0))).Apply())
                {
                    var actualObj1 = container.Resolve().State<string>(0).Instance<Statefull>("a");
                    var actualObj2 = container.Resolve().State<string>(0).Instance<Statefull>("b");

                    // Then
                    actualObj1.ShouldNotBe(actualObj2);
                    actualObj1.Name.ShouldBe("a");
                    actualObj2.Name.ShouldBe("b");
                }
            }
        }

        [Test]
        public void ContainerShouldResolveSingleInstanceWhenSingletoneUsingDifferentGenericContracts()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Contract<ISimpleService>()
                    .Contract<IDisposableService>()
                    .Contract(typeof(IGenericService<>))
                    .Contract(typeof(IGenericService<>), typeof(ISimpleService))
                    .FactoryMethod(ctx => new Stub<string>()).Apply())
                {
                    object actualObj1 = container.Resolve().Instance<ISimpleService>();
                    object actualObj2 = container.Resolve().Instance<IDisposableService>();
                    object actualObj3 = container.Resolve().Instance<IGenericService<string>>();
                    object actualObj4 = container.Resolve().Contract<ISimpleService>().Contract(typeof(IGenericService<string>)).Instance();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                    actualObj1.ShouldNotBe(actualObj3);
                    actualObj3.ShouldBe(actualObj4);
                }
            }
        }

        [Test]
        public void ContainerShouldDisposeAutoDisposingInstanceWhenUnregisteredControlled()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                var mock = new Mock<IDisposableService>();

                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.AutoDisposing)
                    .Contract<IDisposableService>()
                    .FactoryMethod(ctx => mock.Object).Apply())
                {
                    container.Resolve().Instance<IDisposableService>();
                }

                // Then
                mock.Verify(i => i.Dispose(), Times.Once);
            }
        }

        [Test]
        public void ContainerShouldDisposeAutoDisposingInstanceWhenContainerDisposedForControlled()
        {
            // Given
            var mock = new Mock<IDisposableService>();

            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                container.Register()
                    .Lifetime(Wellknown.Lifetime.AutoDisposing)
                    .Contract<IDisposableService>()
                    .FactoryMethod(ctx => mock.Object)
                    .Apply();

                container.Resolve().Instance<IDisposableService>();
            }

            // Then
            mock.Verify(i => i.Dispose(), Times.Once);
        }

        [Test]
        public void ContainerShouldResolveSingleInstanceAndDisposeAutoDisposingWhenAutoDisposingSingletone()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                var mock = new Mock<IDisposableService>();

                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Lifetime(Wellknown.Lifetime.AutoDisposing)
                    .Contract<IDisposableService>()
                    .FactoryMethod(ctx => mock.Object).Apply())
                {
                    var actualObj1 = container.Resolve().Instance<IDisposableService>();
                    var actualObj2 = container.Resolve().Instance<IDisposableService>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                }

                mock.Verify(i => i.Dispose(), Times.Once);
            }
        }

        [Test]
        public void ContainerShouldResolveSingleInstanceWhenPerResolve()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Contract<MyClass>()
                    .FactoryMethod(ctx => new MyClass(ctx.ResolverContext.Container.Resolve().Instance<ISimpleService>(), ctx.ResolverContext.Container.Resolve().Instance<ISimpleService>())).Apply())
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.PerResolve)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => new Mock<ISimpleService>().Object).Apply())
                {
                    var myClass = container.Resolve().Instance<MyClass>();

                    // Then
                    myClass.SimpleService1.ShouldBe(myClass.SimpleService2);
                }
            }
        }

        [Test]
        public void ContainerShouldResolveSingleInstanceWhenPerThread()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Lifetimes).ToSelf())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetime.PerThread)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => new Mock<ISimpleService>().Object).Apply())
                {
                    var simpleService1 = container.Resolve().Instance<ISimpleService>();
                    var simpleService2 = container.Resolve().Instance<ISimpleService>();
                    ISimpleService simpleService3 = null;
                    ISimpleService simpleService4 = null;
                    var thread = new Thread(() =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        simpleService3 = container.Resolve().Instance<ISimpleService>();
                        // ReSharper disable once AccessToDisposedClosure
                        simpleService4 = container.Resolve().Instance<ISimpleService>();
                    });
                    thread.Start();
                    thread.Join();

                    // Then
                    simpleService1.ShouldBe(simpleService2);
                    simpleService3.ShouldNotBeNull();
                    simpleService3.ShouldBe(simpleService4);
                    simpleService1.ShouldNotBe(simpleService3);
                }
            }
        }

        private class Statefull
        {
            public Statefull(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private class MyClass
        {
            public MyClass(ISimpleService simpleService1, ISimpleService simpleService2)
            {
                SimpleService1 = simpleService1;
                SimpleService2 = simpleService2;
            }

            public ISimpleService SimpleService1 { get; }

            public ISimpleService SimpleService2 { get; }

        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        private class Stub : ISimpleService, IDisposableService
        {
            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        private class Stub<T> : ISimpleService, IDisposableService, IGenericService<T>
        {
            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
