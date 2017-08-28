namespace DevTeam.IoC.Tests
{
    using System.IO;
    using Configurations.Json;
    using Newtonsoft.Json;
    using Shouldly;
    using Xunit;

    public class JsonConfigurationTests
    {
        [Fact]
        public void JsonShouldDeserializeAndSerialize()
        {
            // Given
            var serializerSettings = JsonConfiguration.CreateSerializerSettings(Reflection.Shared);
            var eventsConfigurationFile = Path.Combine(TestsExtensions.GetBinDirectory(), "EventsConfiguration.json");
            var json = File.ReadAllText(eventsConfigurationFile);
            var configurationDto = JsonConvert.DeserializeObject<ConfigurationDto>(json, serializerSettings);

            // When
            var json2 = JsonConvert.SerializeObject(configurationDto, serializerSettings);
            var configurationDto2 = JsonConvert.DeserializeObject<ConfigurationDto>(json2, serializerSettings);
            var json3 = JsonConvert.SerializeObject(configurationDto2, serializerSettings);
            // ReSharper disable once UnusedVariable
            var configurationDto3 = JsonConvert.DeserializeObject<ConfigurationDto>(json, serializerSettings);
            var json4 = JsonConvert.SerializeObject(configurationDto2, serializerSettings);

            // Then
            json4.ShouldBe(json3);
        }
    }
}
