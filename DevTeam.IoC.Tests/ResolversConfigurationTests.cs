namespace DevTeam.IoC.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ResolversConfigurationTests
    {
        [Test]
        public void ContainerShouldResolveViaTypedResolver()
        {
            // Given
            var simpleService = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Resolvers).ToSelf())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").FactoryMethod(ctx => simpleService.Object))
                {
                    var resolver = container.Resolve().Tag("abc").Instance<IResolver<ISimpleService>>();
                    var actualObj = resolver.Resolve();

                    // Then
                    actualObj.ShouldBe(simpleService.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldResolveViaTypedResolverWithState()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Resolvers).ToSelf())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").State<string>(0).State<int>(1).Autowiring<StateClass>())
                {
                    var resolver = container.Resolve().Tag("abc").Instance<IResolver<string, int, ISimpleService>>();
                    var actualObj = resolver.Resolve("text", 33) as StateClass;

                    // Then
                    actualObj.ShouldNotBeNull();
                    actualObj.Arg1.ShouldBe("text");
                    actualObj.Arg2.ShouldBe(33);
                }
            }
        }

        [Test]
        public void ContainerShouldResolveViaTypedResolverWithTypedState()
        {
            // Given
            using (var container = CreateContainer())
            using (container.Configure().DependsOn(Wellknown.Feature.Resolvers).ToSelf())
            {
                // When
                using (container.Register().Contract<ISimpleService>().Tag("abc").State<string>(0).State<int>(1).Autowiring<StateClass>())
                {
                    var resolver = container.Resolve().Tag("abc").Instance<IResolver<string, int, ISimpleService>>();
                    var actualObj = resolver.Resolve("text", 33) as StateClass;

                    // Then
                    actualObj.ShouldNotBeNull();
                    actualObj.Arg1.ShouldBe("text");
                    actualObj.Arg2.ShouldBe(33);
                }
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }

        private class Stub<T>: IMultService<T>
        {
            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class StateClass: ISimpleService
        {
            public StateClass([State] string arg1, [State] int arg2)
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            public string Arg1 { get; }

            public int Arg2 { get; }
        }
    }
}