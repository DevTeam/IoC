namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Models;
    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ConfigurationFromAssemblyTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
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
            registrations.OfType<ICompositeKey>().Count(i => i.ContractKeys.Contains(new ContractKey(typeof(ILog), true)) && i.StateKeys.Contains(new StateKey(0, typeof(string)))).ShouldBe(1);
        }

        private static ConfigurationFromAssembly CreateInstance(Assembly assembly)
        {
            return new ConfigurationFromAssembly(assembly);
        }
    }
}
