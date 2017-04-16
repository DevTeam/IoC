namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConverterRegisterDtoToRegistations: IConverter<IRegisterDto, IEnumerable<IRegistrationResult<IContainer>>, ConverterRegisterDtoToRegistations.Context>
    {
        [NotNull] private readonly IReflection _reflection;
        [NotNull] private readonly ITypeResolver _typeResolver;
        [NotNull] private readonly IConverter<ITagDto, object, TypeResolverContext> _converterTagDtoToObject;
        [NotNull] private readonly IConverter<IParameterDto, IParameterMetadata, ConverterParameterDtoToParameterMetadata.Context> _converterParameterDtoToParameterMetadata;

        public ConverterRegisterDtoToRegistations(
            [NotNull] IReflection reflection,
            [NotNull] ITypeResolver typeResolver,
            [NotNull] IConverter<ITagDto, object, TypeResolverContext> converterTagDtoToObject,
            [NotNull] IConverter<IParameterDto, IParameterMetadata, ConverterParameterDtoToParameterMetadata.Context> converterParameterDtoToParameterMetadata)
        {
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (converterTagDtoToObject == null) throw new ArgumentNullException(nameof(converterTagDtoToObject));
            if (converterParameterDtoToParameterMetadata == null) throw new ArgumentNullException(nameof(converterParameterDtoToParameterMetadata));
            _reflection = reflection;
            _typeResolver = typeResolver;
            _converterTagDtoToObject = converterTagDtoToObject;
            _converterParameterDtoToParameterMetadata = converterParameterDtoToParameterMetadata;
        }

        public bool TryConvert(IRegisterDto registerDto, out IEnumerable<IRegistrationResult<IContainer>> value, Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            value = Convert(registerDto, context);
            return true;
        }

        private IEnumerable<IRegistrationResult<IContainer>> Convert(IRegisterDto registerDto, Context context)
        {
            var registration = context.Container.Register();
            foreach (var registerStatementDto in registerDto.Keys ?? Enumerable.Empty<IRegisterStatementDto>())
            {
                var contractDto = registerStatementDto as IContractDto;
                if (contractDto != null)
                {
                    var contractTypes = new List<Type>();
                    foreach (var typeName in contractDto.Contract)
                    {
                        Type contractType;
                        if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, typeName, out contractType))
                        {
                            throw new Exception($"Invalid contract type {typeName}");
                        }

                        contractTypes.Add(contractType);
                    }

                    registration.Contract(contractTypes.ToArray());
                    continue;
                }

                var stateDto = registerStatementDto as IStateDto;
                if (stateDto != null)
                {
                    Type stateType;
                    if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, stateDto.StateTypeName, out stateType))
                    {
                        throw new Exception($"Invalid state type {stateDto.StateTypeName}");
                    }

                    registration.State(stateDto.Index, stateType);
                    continue;
                }

                var tagDto = registerStatementDto as ITagDto;
                if (tagDto != null)
                {
                    object tag;
                    if (!TryGetTagValue(context.TypeResolverContext, tagDto, out tag))
                    {
                        throw new Exception($"Invalid tag {tagDto.Value}");
                    }

                    registration.Tag(tag);
                    continue;
                }

                var lifetimeDto = registerStatementDto as ILifetimeDto;
                if (lifetimeDto != null)
                {
                    registration.Lifetime(lifetimeDto.Lifetime);
                    continue;
                }

                var scopeDto = registerStatementDto as IScopeDto;
                if (scopeDto != null)
                {
                    registration.Scope(scopeDto.Scope);
                    continue;
                }

                var keyComparerDto = registerStatementDto as IKeyComparerDto;
                if (keyComparerDto != null)
                {
                    registration.KeyComparer(keyComparerDto.KeyComparer);
                }
            }

            if (!registerDto.AutowiringTypeName.IsNullOrWhiteSpace())
            {
                Type autowiringType;
                if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, registerDto.AutowiringTypeName, out autowiringType))
                {
                    throw new Exception($"Invalid autowiring type {registerDto.AutowiringTypeName}");
                }

                MethodMetadata constructorMetadata = null;
                if (registerDto.ConstructorParameters != null)
                {
                    var constructorParameters = new List<IParameterMetadata>();
                    var bindingCtorParams = registerDto.ConstructorParameters.ToArray();
                    var stateIndex = 0;
                    foreach (var paramDto in bindingCtorParams)
                    {
                        var param = ResolveParameterMetadata(context.Container, context.TypeResolverContext, paramDto, stateIndex);
                        constructorParameters.Add(param);
                        registration.Key(Enumerable.Repeat<IKey>(param.StateKey, 1));
                        if (!param.IsDependency)
                        {
                            stateIndex++;
                        }
                    }

                    constructorMetadata = new MethodMetadata(".ctor", constructorParameters);
                }

                List<MethodMetadata> methodsMetadata = null;
                if (registerDto.Methods != null)
                {
                    foreach (var methodDto in registerDto.Methods)
                    {
                        var methodParameters = new List<IParameterMetadata>();
                        var bindingCtorParams = methodDto.MethodParameters.ToArray();
                        var stateIndex = 0;
                        foreach (var paramDto in bindingCtorParams)
                        {
                            var param = ResolveParameterMetadata(context.Container, context.TypeResolverContext, paramDto, stateIndex);
                            methodParameters.Add(param);
                            if (!param.IsDependency)
                            {
                                stateIndex++;
                            }
                        }

                        if (methodsMetadata == null)
                        {
                            methodsMetadata = new List<MethodMetadata>();
                        }

                        methodsMetadata.Add(new MethodMetadata(methodDto.Name, methodParameters));
                    }
                }

                List<PropertyMetadata> propertiesMetadata = null;
                if (registerDto.Properties != null)
                {
                    foreach (var propertyDto in registerDto.Properties)
                    {
                        if (propertiesMetadata == null)
                        {
                            propertiesMetadata = new List<PropertyMetadata>();
                        }

                        var param = ResolveParameterMetadata(context.Container, context.TypeResolverContext, propertyDto.Property, 0);
                        propertiesMetadata.Add(new PropertyMetadata(propertyDto.Name, param));
                    }
                }

                var typeMetadata = new TypeMetadata(constructorMetadata, methodsMetadata, propertiesMetadata);
                if (!typeMetadata.IsEmpty)
                {
                    var metadataProvider = context.Container.Resolve().State<TypeMetadata>(0).Instance<IMetadataProvider>(typeMetadata);
                    yield return registration.Autowiring(autowiringType, false, metadataProvider);
                }
                else
                {
                    yield return registration.Autowiring(autowiringType);
                }
            }

            if (!registerDto.FactoryMethodName.IsNullOrWhiteSpace())
            {
                var parts = registerDto.FactoryMethodName.Split(new[] { ".", ":", "::", "->" }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
                if (parts.Length < 2)
                {
                    throw new Exception($"Invalid factory method name {registerDto.FactoryMethodName}");
                }

                var factoryMethodTypeName = string.Join(".", parts.Reverse().Skip(1).Reverse().ToArray());
                var factoryMethodName = parts.Last();
                Type factoryMethodType;
                if (!_typeResolver.TryResolveType(context.TypeResolverContext.References, context.TypeResolverContext.Usings, factoryMethodTypeName, out factoryMethodType))
                {
                    throw new Exception($"Invalid factory method type {factoryMethodName}");
                }

                var factoryMethod = _reflection.GetType(factoryMethodType).GetMethod(factoryMethodName, typeof(ICreationContext));
                if (factoryMethod == null)
                {
                    throw new Exception($"Factory method {registerDto.FactoryMethodName} was not found");
                }

                yield return registration.FactoryMethod(ctx => factoryMethod.Invoke(null, new object[] { ctx }));
            }
        }

        private bool TryGetTagValue([NotNull] TypeResolverContext typeResolverContext, [CanBeNull] ITagDto tagDto, out object tagValue)
        {
            if (typeResolverContext == null) throw new ArgumentNullException(nameof(typeResolverContext));
            return _converterTagDtoToObject.TryConvert(tagDto, out tagValue, typeResolverContext);
        }

        private IParameterMetadata ResolveParameterMetadata(
            [NotNull] IContainer container,
            [NotNull] TypeResolverContext typeResolverContext,
            [NotNull] IParameterDto paramDto,
            int stateIndex)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (paramDto == null) throw new ArgumentNullException(nameof(paramDto));
            if (stateIndex < 0) throw new ArgumentOutOfRangeException(nameof(stateIndex));
            IParameterMetadata paramMetadata;
            if (!_converterParameterDtoToParameterMetadata.TryConvert(paramDto, out paramMetadata, new ConverterParameterDtoToParameterMetadata.Context(container, stateIndex, typeResolverContext)))
            {
                throw new Exception($"Invalid parameter {paramDto.TypeName}");
            }

            return paramMetadata;
        }

        internal sealed class Context
        {
            public Context([NotNull] IContainer container, [NotNull] TypeResolverContext typeResolverContext)
            {
                if (container == null) throw new ArgumentNullException(nameof(container));
                if (typeResolverContext == null) throw new ArgumentNullException(nameof(typeResolverContext));
                Container = container;
                TypeResolverContext = typeResolverContext;
            }

            public IContainer Container { [NotNull] get; }

            public TypeResolverContext TypeResolverContext { [NotNull] get; }
        }
    }
}
