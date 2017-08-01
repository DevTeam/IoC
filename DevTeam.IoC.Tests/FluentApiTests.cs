﻿using DevTeam.IoC.Contracts;

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
                .Autowiring<ISimpleService, MyClass3>()
                .And().Lifetime(Wellknown.Lifetime.Singleton).State<string>(0).With()
                .Tag(1).Autowiring<ISimpleService, MyClass1>()
                .And().Tag(2).Autowiring<ISimpleService, MyClass2>()
                .ToSelf())
            {
                // When
                var obj = container.Resolve().Instance<ISimpleService>();
                var obj2 = container.Resolve().Instance<ISimpleService>();

                // Then
                obj.ShouldBeOfType<MyClass3>();
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        private class MyClass1: ISimpleService
        {
            private static int _counter;

            public MyClass1([State] string str)
            {
                _counter++;
                _counter.ShouldBe(1);
            }
        }

        private class MyClass2 : ISimpleService
        {
            private static int _counter;

            public MyClass2([State] string str)
            {
                _counter++;
                _counter.ShouldBe(1);
            }
        }

        private class MyClass3 : ISimpleService
        {
            public MyClass3(
                [Tag(1), State(0, typeof(string), Value = "abc1")] ISimpleService myClass1,
                [Tag(2), State(0, typeof(string), Value = "abc2")] ISimpleService myClass2)
            {
                myClass1.ShouldBeOfType<MyClass1>();
                myClass2.ShouldBeOfType<MyClass2>();
            }
        }
    }
}