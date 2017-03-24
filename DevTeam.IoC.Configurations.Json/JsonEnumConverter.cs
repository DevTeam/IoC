namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using Newtonsoft.Json;

    internal sealed class JsonEnumConverter<T>: JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Enum.Parse(typeof(T), (string)reader.Value);
        }
    }
}
