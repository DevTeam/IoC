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
        public void ContanireShouldResolveSingleInstanceWhenSingletone()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.Singleton)
                    .Contract<ISimpleService>()
                    .AsFactoryMethod(ctx => new Mock<ISimpleService>().Object))
                {
                    var actualObj1 = container.Resolve().Instance<ISimpleService>();
                    var actualObj2 = container.Resolve().Instance<ISimpleService>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                }
            }
        }

        [Test]
        public void ContanireShouldResolveSingleInstanceWhenSingletoneUsingDifferentContracts()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.Singleton)
                    .Contract<ISimpleService>()
                    .Contract<IDisposableService>()
                    .AsFactoryMethod(ctx => new Stub()))
                {
                    object actualObj1 = container.Resolve().Instance<ISimpleService>();
                    object actualObj2 = container.Resolve().Instance<IDisposableService>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                }
            }
        }

        [Test]
        public void ContanireShouldResolveDifInstancesWhenDifStateAndPerState()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.PerState)
                    .Contract<Statefull>()
                    .State<string>(0)
                    .AsFactoryMethod(ctx => new Statefull(ctx.GetState<string>(0))))
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
        public void ContanireShouldResolveSingleInstanceWhenSingletoneUsingDifferentGenericContracts()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.Singleton)
                    .Contract<ISimpleService>()
                    .Contract<IDisposableService>()
                    .Contract(typeof(IGenericService<>))
                    .Contract(typeof(IGenericService<>), typeof(ISimpleService))
                    .AsFactoryMethod(ctx => new Stub<string>()))
                {
                    object actualObj1 = container.Resolve().Instance<ISimpleService>();
                    object actualObj2 = container.Resolve().Instance<IDisposableService>();
                    object actualObj3 = container.Resolve().Instance<IGenericService<string>>();
                    object actualObj4 = container.Resolve().Contract<ISimpleService>().Instance<IGenericService<string>>();

                    // Then
                    actualObj1.ShouldBe(actualObj2);
                    actualObj1.ShouldNotBe(actualObj3);
                    actualObj3.ShouldBe(actualObj4);
                }
            }
        }

        [Test]
        public void ContanireShouldDisposeAutoDisposingInstanceWhenUnregisteredControlled()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                var mock = new Mock<IDisposableService>();

                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.AutoDisposing)
                    .Contract<IDisposableService>()
                    .AsFactoryMethod(ctx => mock.Object))
                {
                    container.Resolve().Instance<IDisposableService>();
                }

                // Then
                mock.Verify(i => i.Dispose(), Times.Once);
            }
        }

        [Test]
        public void ContanireShouldDisposeAutoDisposingInstanceWhenContainerDisposedForControlled()
        {
            // Given
            var mock = new Mock<IDisposableService>();

            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                container.Register()
                    .Lifetime(Wellknown.Lifetimes.AutoDisposing)
                    .Contract<IDisposableService>()
                    .AsFactoryMethod(ctx => mock.Object);

                container.Resolve().Instance<IDisposableService>();
            }

            // Then
            mock.Verify(i => i.Dispose(), Times.Once);
        }

        [Test]
        public void ContanireShouldResolveSingleInstanceAndDisposeAutoDisposingWhenAutoDisposingSingletone()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                var mock = new Mock<IDisposableService>();

                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.Singleton)
                    .Lifetime(Wellknown.Lifetimes.AutoDisposing)
                    .Contract<IDisposableService>()
                    .AsFactoryMethod(ctx => mock.Object))
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
        public void ContanireShouldResolveSingleInstanceWhenPerResolve()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .AsFactoryMethod(ctx => new MyClass(ctx.Container.Resolve().Instance<ISimpleService>(), ctx.Container.Resolve().Instance<ISimpleService>())))
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.PerResolve)
                    .Contract<ISimpleService>()
                    .AsFactoryMethod(ctx => new Mock<ISimpleService>().Object))
                {
                    var myClass = container.Resolve().Instance<MyClass>();

                    // Then
                    myClass.SimpleService1.ShouldBe(myClass.SimpleService2);
                }
            }
        }

        [Test]
        public void ContanireShouldResolveSingleInstanceWhenPerThread()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Configurations.Lifetimes).Apply())
            {
                // When
                using (
                    container.Register()
                    .Lifetime(Wellknown.Lifetimes.PerThread)
                    .Contract<ISimpleService>()
                    .AsFactoryMethod(ctx => new Mock<ISimpleService>().Object))
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

        public class Statefull
        {
            public Statefull(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class MyClass
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
