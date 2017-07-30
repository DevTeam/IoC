namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConverterParameterDtoToParameterMetadata: IConverter<IParameterDto, IParameterMetadata, ConverterParameterDtoToParameterMetadata.Context>
    {
        [NotNull] private readonly IReflection _reflection;
        [NotNull] private readonly ITypeResolver _typeResolver;
        private readonly IConverter<string, object, Type> _converterStringToObject;
        private readonly IConverter<IValueDto, object, TypeResolverContext> _converterValueDtoToObject;
        private readonly IConverter<ITagDto, object, TypeResolverContext> _converterTagDtoToObject;

        public ConverterParameterDtoToParameterMetadata(
            [NotNull] IReflection reflection,
            [NotNull] ITypeResolver typeResolver,
            [NotNull] IConverter<string, object, Type> converterStringToObject,
            [NotNull] IConverter<IValueDto, object, TypeResolverContext> converterValueDtoToObject,
            [NotNull] IConverter<ITagDto, object, TypeResolverContext> converterTagDtoToObject)
        {
            _reflection = reflection ?? throw new ArgumentNullException(nameof(reflection));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            _converterStringToObject = converterStringToObject ?? throw new ArgumentNullException(nameof(converterStringToObject));
            _converterValueDtoToObject = converterValueDtoToObject ?? throw new ArgumentNullException(nameof(converterValueDtoToObject));
            _converterTagDtoToObject = converterTagDtoToObject ?? throw new ArgumentNullException(nameof(converterTagDtoToObject));
        }

        public bool TryConvert(IParameterDto paramDto, out IParameterMetadata paramMetadata, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (paramDto == null)
            {
                paramMetadata = default(IParameterMetadata);
                return false;
            }

            if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, paramDto.TypeName, out Type parameterType))
            {
                throw new Exception($"Invalid parameter type {paramDto.TypeName}");
            }

            Resolving<IContainer> resolving = null;
            IStateKey stateKey = null;
            object value = null;
            var state = new List<object>();
            if (paramDto.Value != null)
            {
                if (!_converterStringToObject.TryConvert(paramDto.Value, out value, parameterType))
                {
                    throw new Exception($"Invalid value \"{paramDto.Value}\" of type {parameterType.Name}");
                }
            }
            else if (paramDto.State != null)
            {
                if (paramDto.State != null)
                {
                    if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, paramDto.State.StateTypeName, out Type stateType))
                    {
                        throw new Exception($"Invalid state type {paramDto.State.StateTypeName}");
                    }

                    if (paramDto.State.Value != null)
                    {
                        if (!_converterValueDtoToObject.TryConvert(paramDto.State.Value, out value, context.TypeResolverContext))
                        {
                            throw new Exception($"Invalid state {paramDto.State.Value.Data}");
                        }

                        state.Add(value);
                    }

                    stateKey = new StateKey(_reflection, paramDto.State.Index, stateType, true);
                }
            }
            else
            {
                if (paramDto.Dependency != null)
                {
                    resolving = new Resolving<IContainer>(context.Container);
                    var hasContractKey = false;
                    foreach (var keyDto in paramDto.Dependency)
                    {
                        switch (keyDto)
                        {

                            case IContractDto contractDto:
                                var contractTypes = new List<Type>();
                                foreach (var typeName in contractDto.Contract)
                                {
                                    if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, typeName, out Type contractType))
                                    {
                                        throw new Exception($"Invalid contract type {typeName}");
                                    }

                                    contractTypes.Add(contractType);
                                }

                                if (contractTypes.Count == 0)
                                {
                                    contractTypes.Add(parameterType);
                                }

                                resolving.Contract(contractTypes.ToArray());
                                hasContractKey = true;
                                break;

                            case IStateDto stateDto:
                                if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, stateDto.StateTypeName, out Type stateType))
                                {
                                    throw new Exception($"Invalid state type {stateDto.StateTypeName}");
                                }

                                if (stateDto.Value != null)
                                {
                                    if (!_converterValueDtoToObject.TryConvert(stateDto.Value, out object stetItem, context.TypeResolverContext))
                                    {
                                        throw new Exception($"Invalid state {stateDto.Value.Data}");
                                    }

                                    state.Add(stetItem);
                                }

                                resolving.State(stateDto.Index, stateType);
                                break;

                            case ITagDto tagDto:
                                if (!_converterTagDtoToObject.TryConvert(tagDto, out object tag, context.TypeResolverContext))
                                {
                                    throw new Exception($"Invalid tag {tagDto.Value}");
                                }

                                resolving.Tag(tag);
                                break;
                        }
                    }

                    if (!hasContractKey)
                    {
                        resolving.Contract(parameterType);
                    }
                }
            }

            paramMetadata = new ParameterMetadata(
                resolving?.ContractKeys.ToArray() ?? new IContractKey[] { new ContractKey(_reflection, parameterType, true) },
                resolving?.TagKeys?.ToArray(),
                resolving?.StateKeys?.ToArray(),
                context.StateIndex,
                state.ToArray(),
                value,
                stateKey);

            return true;
        }

        internal sealed class Context
        {
            public Context([NotNull] IContainer container, int stateIndex, [NotNull] TypeResolverContext typeResolverContext)
            {
                Container = container ?? throw new ArgumentNullException(nameof(container));
                StateIndex = stateIndex;
                TypeResolverContext = typeResolverContext ?? throw new ArgumentNullException(nameof(typeResolverContext));
            }

            public IContainer Container { get; }

            public int StateIndex { get; }

            public TypeResolverContext TypeResolverContext { get; }
        }
    }
}
