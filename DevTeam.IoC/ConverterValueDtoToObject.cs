namespace DevTeam.IoC
{
    using System;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConverterValueDtoToObject: IConverter<IValueDto, object, TypeResolverContext>
    {
        [NotNull] private readonly ITypeResolver _typeResolver;
        [NotNull] private readonly IConverter<string, object, Type> _converterStringToObject;

        public ConverterValueDtoToObject(
            [NotNull] ITypeResolver typeResolver,
            [NotNull] IConverter<string, object, Type> converterStringToObject)
        {
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            _converterStringToObject = converterStringToObject ?? throw new ArgumentNullException(nameof(converterStringToObject));
        }

        public bool TryConvert(IValueDto valueDto, out object value, TypeResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (valueDto == null)
            {
                value = default(object);
                return false;
            }

            if (!_typeResolver.TryResolveType(context.References, context.Usings, valueDto.TypeName, out Type type))
            {
                type = typeof(string);
            }

            return _converterStringToObject.TryConvert(valueDto.Data, out value, type);
        }
    }
}
