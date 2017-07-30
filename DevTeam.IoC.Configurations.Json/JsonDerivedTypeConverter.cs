namespace DevTeam.IoC.Configurations.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class JsonDerivedTypeConverter<T>: JsonConverter
    {
        [NotNull] private readonly IReflection _reflection;
        [NotNull] private readonly IDictionary<Type, string[]> _derivedTypes;

        public JsonDerivedTypeConverter([NotNull] IReflection reflection, params Type[] derivedTypes)
        {
            if (derivedTypes == null) throw new ArgumentNullException(nameof(derivedTypes));
            if (derivedTypes.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(derivedTypes));
            _reflection = reflection ?? throw new ArgumentNullException(nameof(reflection));
            _derivedTypes = derivedTypes.ToDictionary(i => i, GetPropertiesNames);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var names = jsonObject.Properties().Select(i => i.Name).ToArray();

            var type =  (
                from derivedType in _derivedTypes
                where !names.Except(derivedType.Value, StringComparer.OrdinalIgnoreCase).Any()
                select new
                {
                    derivedType,
                    cnt = derivedType.Value.Intersect(names, StringComparer.OrdinalIgnoreCase).Count()
                })
                .OrderByDescending(i => i.cnt)
                .Select(i => i.derivedType.Key)
                .FirstOrDefault();

                if (type != null)
                {
                    return jsonObject.ToObject(type, serializer);
                }

            throw new InvalidOperationException("Type was not found");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        private string[] GetPropertiesNames(Type type)
        {
            var names = (
                from curType in GetTypes(type)
                from prop in _reflection.GetType(curType).Properties
                let jsonIgnoreAttribute = _reflection.GetCustomAttributes<JsonIgnoreAttribute>(prop, false).FirstOrDefault()
                where jsonIgnoreAttribute == null
                let jsonPropertyAttribute = _reflection.GetCustomAttributes<JsonPropertyAttribute>(prop, false).FirstOrDefault()
                select jsonPropertyAttribute?.PropertyName ?? prop.Name).Distinct().ToArray();

            if (names.Length == 0)
            {
                throw new InvalidOperationException($"{type.Name} does not containt any properties");
            }

            return names;
        }

        private IEnumerable<Type> GetTypes(Type type)
        {
            do {
                yield return type;
                type = _reflection.GetType(type).BaseType;
            }
            while (type != null && type != typeof(object));
        }
    }
}
