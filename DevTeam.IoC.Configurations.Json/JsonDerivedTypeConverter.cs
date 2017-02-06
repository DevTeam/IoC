namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class JsonDerivedTypeConverter<T>: JsonConverter
    {
        private readonly IDictionary<Type, string[]> _derivedTypes;

        public JsonDerivedTypeConverter(params Type[] derivedTypes)
        {
            if (derivedTypes == null) throw new ArgumentNullException(nameof(derivedTypes));
            if (derivedTypes.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(derivedTypes));
            _derivedTypes = derivedTypes.ToDictionary(i => i, GetPropertiesNames);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var type =  (
                from derivedType in _derivedTypes
                select new
                {
                    derivedType,
                    cnt = (
                        from name in derivedType.Value
                        join propName in jsonObject.Properties().Select(i => i.Name) on name equals propName
                        select propName).Count()
                })
                .OrderByDescending(i => i.cnt)
                .Select(i => i.derivedType.Key)
                .FirstOrDefault();

                if (type != null)
                {
                    return jsonObject.ToObject(type, serializer);
                }

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        private static string[] GetPropertiesNames(Type type)
        {
            var names = (
                from prop in type.GetTypeInfo().DeclaredProperties
                let jsonIgnoreAttribute = prop.GetCustomAttribute(typeof(JsonIgnoreAttribute), true) as JsonIgnoreAttribute
                where jsonIgnoreAttribute == null
                let jsonPropertyAttribute = prop.GetCustomAttribute(typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute
                select jsonPropertyAttribute?.PropertyName ?? prop.Name).ToArray();

            if (names.Length == 0)
            {
                throw new InvalidOperationException($"{type.Name} does not containt any properties");
            }

            return names;
        }
    }
}
