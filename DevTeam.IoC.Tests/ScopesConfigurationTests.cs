namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    public class ScopesConfigurationTests
    {
        [Test]
        public void ContainerShouldResolveWhenGlobalScope()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (var childContainer = new Container(container, "child"))
            using (container.Configure().DependsOn(Wellknown.Feature.Scopes).ToSelf())
            {
                // When
                using (
                    childContainer.Register()
                    .Scope(Wellknown.Scope.Global)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => mock.Object)
                    .Apply())
                {
                    var actualObj = container.Resolve().Instance<ISimpleService>();

                    // Then
                    actualObj.ShouldBe(mock.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldUnregisterWhenGlobalScope()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (var childContainer = new Container(container, "child"))
            using (container.Configure().DependsOn(Wellknown.Feature.Scopes).ToSelf())
            {
                // When
                var registration =
                    childContainer.Register()
                        .Scope(Wellknown.Scope.Global)
                        .Contract<ISimpleService>()
                        .FactoryMethod(ctx => mock.Object)
                        .Apply();

                registration.Dispose();

                var hasRegistration = (
                    from reg in container.Registrations
                    let key = reg as ICompositeKey
                    from contract in key.ContractKeys
                    where contract.ContractType == typeof(ISimpleService)
                    select contract).Any();

                // Then
                hasRegistration.ShouldBeFalse();
            }
        }

        [Test]
        public void ContainerShouldNotResolveWhenInternalScopeAndChildContainer()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (var childContainer1 = new Container(container, "child1"))
            using (var childContainer2 = new Container(container, "child2"))
            using (container.Configure().DependsOn(Wellknown.Feature.Scopes).ToSelf())
            {
                // When
                using (
                    childContainer1.Register()
                    .Scope(Wellknown.Scope.Internal)
                    .Contract<ISimpleService>()
                    .FactoryMethod(ctx => mock.Object).Apply())
                {
                    ISimpleService actualObj;

                    // Then
                    childContainer2.TryResolve(out actualObj).ShouldBeFalse();
                }
            }
        }

        [Test]
        public void ContainerShouldUnregisterWhenInternalScope()
        {
            // Given
            var mock = new Mock<ISimpleService>();
            using (var container = CreateContainer())
            using (var childContainer1 = new Container(container, "child1"))
            // ReSharper disable once UnusedVariable
            using (var childContainer2 = new Container(container, "child2"))
            using (container.Configure().DependsOn(Wellknown.Feature.Scopes).ToSelf())
            {
                // When
                var registration =
                    childContainer1.Register()
                        .Scope(Wellknown.Scope.Internal)
                        .Contract<ISimpleService>()
                        .FactoryMethod(ctx => mock.Object)
                        .Apply();

                registration.Dispose();

                var hasRegistration = (
                   from reg in container.Registrations
                   let key = reg as ICompositeKey
                   from contract in key.ContractKeys
                   where contract.ContractType == typeof(ISimpleService)
                   select contract).Any();

                // Then
                hasRegistration.ShouldBeFalse();
            }
        }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}
