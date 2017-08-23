namespace DevTeam.IoC.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public class CircularDependencyTests
    {
        [Fact]
        public void ShouldResolveCircularDependency()
        {
            // Given
            using (var container = new Container().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                // When
                using (container.Register()
                    .Autowiring<ISimpleService, SimpleService>()
                    .And().Autowiring(typeof(IGenericService<>), typeof(GenericService<>))
                    .And().Autowiring<IDisposableService, DisposableService>())
                {
                    // Then
                    Assert.Throws<ContainerException>(() => container.Resolve().Instance<ISimpleService>());
                }
            }
        }

        private class SimpleService: ISimpleService
        {
            public SimpleService(IGenericService<int> genericService)
            {
            }
        }

        private class GenericService<T> : IGenericService<T>
        {
            public GenericService(IDisposableService disposableService)
            {
            }
        }

        private class DisposableService : IDisposableService
        {
            public DisposableService(ISimpleService simpleService)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
