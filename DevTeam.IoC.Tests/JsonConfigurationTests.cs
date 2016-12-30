namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Configurations.Json;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    public class JsonConfigurationTests
    {
        [Test]
        public void JsonShouldDeserializeAndSerialize()
        {
            // Given
            var eventsConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventsConfiguration.json");
            var json = File.ReadAllText(eventsConfigurationFile);
            var configurationDto = JsonConvert.DeserializeObject<ConfigurationDto>(json, JsonConfiguration.SerializerSettings);

            // When
            var json2 = JsonConvert.SerializeObject(configurationDto, JsonConfiguration.SerializerSettings);
            var configurationDto2 = JsonConvert.DeserializeObject<ConfigurationDto>(json2, JsonConfiguration.SerializerSettings);
            var json3 = JsonConvert.SerializeObject(configurationDto2, JsonConfiguration.SerializerSettings);

            // Then
            json3.ShouldBe(json2);
        }
    }
}
