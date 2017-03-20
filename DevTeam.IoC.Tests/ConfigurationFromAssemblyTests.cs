namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Models;
    using Shouldly;
    using Xunit;

    public class ConfigurationFromAssemblyTests
    {
        private readonly Reflection _reflection = new Reflection();

        [Fact]
        public void ShouldRegister()
        {
            // Given
            var config = CreateInstance(typeof(Console).Assembly);
            var container = 
                new Container().Configure().DependsOn(Wellknown.Feature.ChildContainers).ToSelf()
                .CreateChild().Configure().DependsOn(config).ToSelf();

            // When
            var registrations = container.Registrations.ToList();

            // Then
            registrations.OfType<ICompositeKey>().Count(i => i.ContractKeys.Contains(new ContractKey(new Reflection(), typeof(ILog), true)) && i.StateKeys.Contains(new StateKey(_reflection, 0, typeof(string), true))).ShouldBe(1);
        }

        private static ConfigurationFromAssembly CreateInstance(Assembly assembly)
        {
            return new ConfigurationFromAssembly(assembly);
        }
    }
}
