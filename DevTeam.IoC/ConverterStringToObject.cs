namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class ConverterStringToObject: IConverter<string, object, Type>
    {
        private readonly IReflection _reflection;

        public ConverterStringToObject([NotNull] IReflection reflection)
        {
            _reflection = reflection ?? throw new ArgumentNullException(nameof(reflection));
        }

        public bool TryConvert(string valueText, out object value, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (valueText == null)
            {
                value = default(object);
                return false;
            }

            if (_reflection.GetType(type).IsEnum)
            {
                value = Enum.Parse(type, valueText);
                return true;
            }

            if (type == typeof(TimeSpan))
            {
                value = TimeSpan.Parse(valueText);
                return true;
            }

            value = Convert.ChangeType(valueText, type);
            return true;
        }
    }
}
