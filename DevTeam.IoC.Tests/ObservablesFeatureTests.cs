#if !NET35
namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ObservablesFeatureTests
    {
        [Test]
        public void ShouldSupportResolvingObservable()
        {
            // Given
            var simpleService1 = new Mock<ISimpleService>();
            var simpleService2 = new Mock<ISimpleService>();
            var simpleService3 = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Observables).Apply())
            {
                // When
                using (container.Register().Contract<ISimpleService>().FactoryMethod(ctx => simpleService1.Object).Apply())
                using (container.Register().Tag(1).Contract<ISimpleService>().FactoryMethod(ctx => simpleService2.Object).Apply())
                using (container.Register().Tag("a").Contract<ISimpleService>().FactoryMethod(ctx => simpleService3.Object).Apply())
                {
                    var actualList = ToList(container.Resolve().Instance<IObservable<ISimpleService>>());

                    // Then
                    actualList.Count.ShouldBe(3);
                    actualList.ShouldBe(new []{simpleService1.Object, simpleService2.Object , simpleService3.Object });
                }
            }
        }

        private static IContainer CreateContainer()
        {
            return new Container();
        }

        private static IList<T> ToList<T>(IObservable<T> observable)
        {
            var observer = new Observer<T>();
            observable.Subscribe(observer);
            return observer;
        }

        private class Observer<T> : List<T>, IObserver<T>
        {
            public void OnNext(T value)
            {
                Add(value);
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnCompleted()
            {
            }
        }
    }
}
#endif