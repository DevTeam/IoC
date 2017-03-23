﻿namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Configurations.Json;
    using Newtonsoft.Json;
    using Shouldly;
    using Xunit;
    using DevTeam.IoC.Contracts;

    public class JsonConfigurationTests
    {
        [Fact]
        public void JsonShouldDeserializeAndSerialize()
        {
            // Given
            var serializerSettings = JsonConfiguration.CreateSerializerSettings(Reflection.Shared);
            var eventsConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventsConfiguration.json");
            var json = File.ReadAllText(eventsConfigurationFile);
            var configurationDto = JsonConvert.DeserializeObject<ConfigurationDto>(json, serializerSettings);

            // When
            var json2 = JsonConvert.SerializeObject(configurationDto, serializerSettings);
            var configurationDto2 = JsonConvert.DeserializeObject<ConfigurationDto>(json2, serializerSettings);
            var json3 = JsonConvert.SerializeObject(configurationDto2, serializerSettings);
            var configurationDto3 = JsonConvert.DeserializeObject<ConfigurationDto>(json, serializerSettings);
            var json4 = JsonConvert.SerializeObject(configurationDto2, serializerSettings);

            // Then
            json4.ShouldBe(json3);
        }
    }
}
