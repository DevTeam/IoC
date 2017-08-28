namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class FluentApiTests
    {
        [Fact]
        public void CreateNewRegistrationWhenUseAndFunc()
        {
            // Given
            using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                // When
                var registration1 = (Registration<IContainer>)container.Register().Lifetime(Wellknown.Lifetime.PerState).Tag(1).Contract<ISimpleService>();
                var registration2 = (Registration<IContainer>)registration1.New().Lifetime(Wellknown.Lifetime.PerContainer).Tag(2).Contract<ISimpleService>();

                // Then
                registration1.ShouldNotBe(registration2);
                registration1.Keys.ShouldNotBe(registration2.Keys);
                registration1.Extensions.ShouldNotBe(registration2.Extensions);
                registration2.Keys.Count().ShouldBe(1);
                registration2.Extensions.Count.ShouldBe(1);
            }
        }

        [Fact]
        public void UsePredefinedRegistrationWhenWithFunc()
        {
            // Given
            using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                // When
                var registration1 = (Registration<IContainer>)container.Register().Lifetime(Wellknown.Lifetime.PerState).Tag(1).Contract<ISimpleService>();
                var registration2 = (Registration<IContainer>)registration1.With();

                // Then
                registration1.ShouldNotBe(registration2);
                registration1.Keys.ShouldBe(registration2.Keys);
                registration1.Extensions.ShouldBe(registration2.Extensions);
                registration2.Keys.Count().ShouldBe(1);
                registration2.Extensions.Count.ShouldBe(1);
            }
        }

        [Fact]
        public void UsePredefinedRegistrationViaExtensionsWhenWithFunc()
        {
            // Given
            using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register()
                .Lifetime(Wellknown.Lifetime.Singleton).With()
                .Autowiring<MyClass3, MyClass3>()
                .And().Autowiring<ISimpleService, MyClass1>()
                .And().Autowiring<IDisposableService, MyClass2>()
                .ToSelf())
            {
                // When
                var obj = container.Resolve().Instance<MyClass3>();
                var obj2 = container.Resolve().Instance<MyClass3>();

                // Then
                obj.ShouldBe(obj2);
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        private class MyClass1: ISimpleService
        {
            private static int _counter;

            public MyClass1()
            {
                _counter++;
                _counter.ShouldBe(1);
            }
        }

        private class MyClass2 : IDisposableService
        {
            private static int _counter;

            public MyClass2()
            {
                _counter++;
                _counter.ShouldBe(1);
            }

            public void Dispose()
            {
            }
        }

        private class MyClass3
        {
            public MyClass3(
                ISimpleService myClass1,
                IDisposableService myClass2)
            {
                myClass1.ShouldBeOfType<MyClass1>();
                myClass2.ShouldBeOfType<MyClass2>();
            }
        }
    }
}