// ReSharper disable RedundantUsingDirective
namespace DevTeam.IoC.Tests
{
    using System;
    using System.Linq;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class RootContainerConfigurationTests
    {
#if !NET35
        [Theory]
        [InlineData(typeof(IResolver))]
        [InlineData(typeof(IRegistry))]
        [InlineData(typeof(IKeyFactory))]
        [InlineData(typeof(IFluent))]
        [InlineData(typeof(IReflection))]
        [InlineData(typeof(IMethodFactory))]
        [InlineData(typeof(IMetadataProvider))]
        [InlineData(typeof(RegistryContext))]
        [InlineData(typeof(CreationContext))]
        [InlineData(typeof(ResolverContext))]
        [InlineData(typeof(RegistryContext))]
        public void ShouldRegisterContracts(Type type)
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                var instance = container.Resolve().Contract(type).Instance();

                // Then
                instance.ShouldNotBeNull();
            }
        }
#endif

        [Fact]
        public void ShouldRegisterAllFeatures()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                var features = Enum.GetValues(typeof(Wellknown.Feature)).Cast<Wellknown.Feature>();

                // Then
                // ReSharper disable once AccessToDisposedClosure
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                features.Select(feature => container.Resolve().Tag(feature).Instance<IConfiguration>()).ToArray();
            }
        }

        private static IContainer CreateContainer()
        {
            return new Container();
        }
    }
}