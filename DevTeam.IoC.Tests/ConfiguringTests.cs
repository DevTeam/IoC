namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class ConfiguringTests
    {
        private readonly Container _container = new Container();

        [Fact]
        public void ShouldApplyConfigsAccordingToOrder()
        {
            // Given
            var instance = CreateInstance();
            var ids = new List<string>();
            var configuring = instance.DependsOn(new Config("1", ids), new Config("2", ids));

            // When
            var config = instance.Create();
            using (configuring.Apply())
            {
            }

            // Then
            ids.ShouldBe(new []{ "1", "2" });
            config.GetDependencies(_container).Cast<Config>().Select(i => i.Id).ShouldBe(new[] { "1", "2" });
        }

        [Fact]
        public void ShouldApplyConfigsWhenToSelf()
        {
            // Given
            var instance = CreateInstance();
            var ids = new List<string>();

            // When
            instance.DependsOn(new Config("1", ids), new Config("2", ids)).ToSelf();

            // Then
            ids.ShouldBe(new[] { "1", "2" });
        }

        [Fact]
        public void ShouldDistinctWhenApplyConfigs()
        {
            // Given
            var instance = CreateInstance();
            var ids = new List<string>();
            var config1 = new Config("1", ids);
            var configuring = instance.DependsOn(config1, config1, new Config("2", ids, config1, config1), config1);

            // When
            var config = instance.Create();
            using (configuring.Apply())
            {
            }

            // Then
            ids.ShouldBe(new[] { "1", "2" });
            config.GetDependencies(_container).Cast<Config>().Select(i => i.Id).ShouldBe(new[] { "1", "2" });
        }

        [Fact]
        public void ShouldAppytWhenCycleInDependencies()
        {
            // Given
            var instance = CreateInstance();
            var ids = new List<string>();
            var config1 = new Config("1", ids);
            config1.AddDependency(config1);
            var config2 = new Config("2", ids, config1);
            config2.AddDependency(config2);

            // When
            using (instance.DependsOn(config1, config2).Apply())
            {
            }

            // Then
            ids.ShouldBe(new[] { "1", "2" });
        }

        private Configuring<IContainer> CreateInstance()
        {
            return new Configuring<IContainer>(_container);
        }

        private class Config: IConfiguration
        {
            private readonly List<string> _ids;
            private readonly List<IConfiguration> _dependencies;

            public Config(string id, List<string> ids, params IConfiguration[] dependencies)
            {
                Id = id;
                _ids = ids;
                _dependencies = new List<IConfiguration>(dependencies);
            }

            public string Id { get; }

            public IEnumerable<IConfiguration> GetDependencies(IContainer container)
            {
                return _dependencies;
            }

            public IEnumerable<IDisposable> Apply(IContainer container)
            {
                _ids.Add(Id);
                yield break;
            }

            public void AddDependency(IConfiguration configuration)
            {
                _dependencies.Add(configuration);
            }
        }
    }
}
