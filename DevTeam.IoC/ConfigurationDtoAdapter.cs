namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Dto;

    internal class ConfigurationDtoAdapter : IConfiguration
    {
        private readonly IConfigurationDto _configurationDto;

        internal IConfigurationDto ConfigurationDto => _configurationDto;

        public ConfigurationDtoAdapter([State] IConfigurationDto configurationDto)
        {
            if (configurationDto == null) throw new ArgumentNullException(nameof(configurationDto));
            _configurationDto = configurationDto;
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var typeResolver = resolver.Resolve().Instance<ITypeResolver>();
            foreach (var configurationStatement in _configurationDto)
            {
                var referenceDto = configurationStatement as IReferenceDto;
                if (referenceDto != null)
                {
                    typeResolver.AddReference(referenceDto.Reference);
                    continue;
                }

                var usingDto = configurationStatement as IUsingDto;
                if (usingDto != null)
                {
                    typeResolver.AddUsingStatement(usingDto.Using);
                    continue;
                }

                var dependencyFeatureDto = configurationStatement as IDependencyFeatureDto;
                if (dependencyFeatureDto != null)
                {
                    yield return resolver.Feature(dependencyFeatureDto.Feature);
                    continue;
                }

                var dependencyConfigurationDto = configurationStatement as IDependencyConfigurationDto;
                if (dependencyConfigurationDto != null)
                {
                    Type configurationType;
                    if (!typeResolver.TryResolveType(dependencyConfigurationDto.ConfigurationTypeName, out configurationType) || !typeof(IConfiguration).GetTypeInfo().IsAssignableFrom(configurationType.GetTypeInfo()))
                    {
                        throw new Exception($"Invalid configuration type {configurationType}");
                    }

                    using (var childContainer = resolver.CreateChild())
                    {
                        childContainer.Register().Contract<IConfiguration>().AsAutowiring(configurationType);
                        yield return childContainer.Resolve().Instance<IConfiguration>();
                    }

                    continue;
                }

                var dependencyReferenceDto = configurationStatement as IDependencyReferenceDto;
                if (dependencyReferenceDto != null)
                {
                    Type configurationType;
                    if (!typeResolver.TryResolveType(dependencyReferenceDto.ConfigurationTypeName, out configurationType) || !typeof(IConfiguration).GetTypeInfo().IsAssignableFrom(configurationType.GetTypeInfo()))
                    {
                        throw new Exception($"Invalid configuration type {configurationType}");
                    }

                    var childContainer = resolver.CreateChild();
                    var referenceDescriptionResolver = childContainer.Resolve().Instance<IReferenceDescriptionResolver>();
                    var reference = referenceDescriptionResolver.ResolveReference(dependencyReferenceDto.Reference);
                    var configurationDescriptionDto = childContainer.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(reference);
                    var configurationDto = childContainer.Resolve().Tag(configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
                    yield return new ConfigurationDtoAdapter(configurationDto);
                }
            }
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            return Apply(resolver, resolver.Resolve().Instance<ITypeResolver>(), _configurationDto);
        }

        private IEnumerable<IDisposable> Apply(IResolver resolver, ITypeResolver typeResolver, IEnumerable<IConfigurationStatementDto> configurationStatemens)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            foreach (var configurationStatement in configurationStatemens)
            {
                var referenceDto = configurationStatement as IReferenceDto;
                if (referenceDto != null)
                {
                    typeResolver.AddReference(referenceDto.Reference);
                    continue;
                }

                var usingDto = configurationStatement as IUsingDto;
                if (usingDto != null)
                {
                    typeResolver.AddUsingStatement(usingDto.Using);
                    continue;
                }

                var containerDto = configurationStatement as IContainerDto;
                if (containerDto != null)
                {
                    object containerTag = null;
                    if (containerDto.Tag != null)
                    {
                        if (!TryGetTagValue(typeResolver, containerDto.Tag, out containerTag))
                        {
                            throw new Exception($"Invalid container tag {containerDto.Tag.Value}");
                        }
                    }

                    foreach(var registration in Apply(resolver.CreateChild(containerTag), typeResolver, containerDto.Statements))
                    {
                        yield return registration;
                    }

                    continue;
                }

                var registerDto = configurationStatement as IRegisterDto;
                if (registerDto != null)
                {
                    HandleRegisterDto(resolver, typeResolver, registerDto);
                }
            }
        }

        private void HandleRegisterDto(IResolver resolver, ITypeResolver typeResolver, IRegisterDto registerDto)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (registerDto == null) throw new ArgumentNullException(nameof(registerDto));
            var registration = resolver.Register();
            foreach (var registerStatementDto in registerDto.Keys ?? Enumerable.Empty<IRegisterStatementDto>())
            {
                var contractDto = registerStatementDto as IContractDto;
                if (contractDto != null)
                {
                    var contractTypes = new List<Type>();
                    foreach (var typeName in contractDto.Contract)
                    {
                        Type contractType;
                        if (!typeResolver.TryResolveType(typeName, out contractType))
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
                    if (!typeResolver.TryResolveType(stateDto.StateTypeName, out stateType))
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
                    if (!TryGetTagValue(typeResolver, tagDto, out tag))
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

            if (!string.IsNullOrWhiteSpace(registerDto.AutowiringTypeName))
            {
                Type autowiringType;
                if (!typeResolver.TryResolveType(registerDto.AutowiringTypeName, out autowiringType))
                {
                    throw new Exception($"Invalid autowiring type {registerDto.AutowiringTypeName}");
                }

                IMetadataProvider metadataProvider = null;
                if (registerDto.ConstructorParameters != null)
                {
                    var constructorParameters = new List<IParameterMetadata>();
                    var bindingCtorParams = registerDto.ConstructorParameters.ToArray();
                    var stateIndex = 0;
                    foreach (var ctorParam in bindingCtorParams)
                    {
                        Type parameterType;
                        if (!typeResolver.TryResolveType(ctorParam.TypeName, out parameterType))
                        {
                            throw new Exception($"Invalid constructor parameter type {ctorParam.TypeName}");
                        }

                        IStateKey stateKey = null;
                        IKey key = null;
                        object value = null;
                        var state = new List<object>();
                        if (ctorParam.Value != null)
                        {
                            if (!TryGetValue(parameterType, ctorParam.Value, out value))
                            {
                                throw new Exception($"Invalid value \"{ctorParam.Value}\" of type {parameterType.Name}");
                            }
                        }
                        else
                        if (ctorParam.State != null)
                        {
                            if (ctorParam.State != null)
                            {
                                Type stateType;
                                if (!typeResolver.TryResolveType(ctorParam.State.StateTypeName, out stateType))
                                {
                                    throw new Exception($"Invalid state type {ctorParam.State.StateTypeName}");
                                }

                                if (ctorParam.State.Value != null)
                                {
                                    if (!TryGetValue(typeResolver, ctorParam.State.Value, out value))
                                    {
                                        throw new Exception($"Invalid state {ctorParam.State.Value.Data}");
                                    }

                                    state.Add(value);
                                }

                                stateKey = new StateKey(ctorParam.State.Index, stateType);
                            }
                        }
                        else
                        {
                            if (ctorParam.Dependency != null)
                            {
                                var resolving = new Resolving(resolver.Resolve().Instance<IFluent>(), resolver);
                                var hasContractKey = false;
                                foreach (var keyDto in ctorParam.Dependency)
                                {
                                    var contractDto = keyDto as IContractDto;
                                    if (contractDto != null)
                                    {
                                        var contractTypes = new List<Type>();
                                        foreach (var typeName in contractDto.Contract)
                                        {
                                            Type contractType;
                                            if (!typeResolver.TryResolveType(typeName, out contractType))
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
                                    }

                                    var stateDto = keyDto as IStateDto;
                                    if (stateDto != null)
                                    {
                                        Type stateType;
                                        if (!typeResolver.TryResolveType(stateDto.StateTypeName, out stateType))
                                        {
                                            throw new Exception($"Invalid state type {stateDto.StateTypeName}");
                                        }

                                        if (stateDto.Value != null)
                                        {
                                            object stetItem;
                                            if (!TryGetValue(typeResolver, stateDto.Value, out stetItem))
                                            {
                                                throw new Exception($"Invalid state {stateDto.Value.Data}");
                                            }

                                            state.Add(stetItem);
                                        }

                                        resolving.State(stateDto.Index, stateType);
                                    }

                                    var tagDto = keyDto as ITagDto;
                                    if (tagDto != null)
                                    {
                                        object tag;
                                        if (!TryGetTagValue(typeResolver, tagDto, out tag))
                                        {
                                            throw new Exception($"Invalid tag {tagDto.Value}");
                                        }

                                        resolving.Tag(tag);
                                    }
                                }

                                if (!hasContractKey)
                                {
                                    resolving.Contract(parameterType);
                                }
                                
                                key = resolving.CreateCompositeKey();
                            }
                            else
                            {
                                key = new ContractKey(parameterType, true);
                            }
                        }

                        var param = new ParameterMetadata(stateKey == null ? new[] { key } : null, stateIndex, state.ToArray(), value, stateKey);
                        constructorParameters.Add(param);
                        if (!param.IsDependency)
                        {
                            stateIndex++;
                        }
                    }

                    metadataProvider = resolver.Resolve().State<IEnumerable<IParameterMetadata>>(0).Instance<IMetadataProvider>(constructorParameters);
                }

                registration.AsAutowiring(autowiringType, metadataProvider);
                return;
            }

            if (!string.IsNullOrWhiteSpace(registerDto.FactoryMethodName))
            {
                var parts = registerDto.FactoryMethodName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    throw new Exception($"Invalid factory method name {registerDto.FactoryMethodName}");
                }

                var factoryMethodTypeName = string.Join(".", parts.Reverse().Skip(1).Reverse());
                var factoryMethodName = parts.Last();
                Type factoryMethodType;
                if (!typeResolver.TryResolveType(factoryMethodTypeName, out factoryMethodType))
                {
                    throw new Exception($"Invalid factory method type {factoryMethodName}");
                }

                var factoryMethod = factoryMethodType.GetRuntimeMethod(factoryMethodName, new[] { typeof(IResolverContext) });
                if (factoryMethod == null)
                {
                    throw new Exception($"Factory method {registerDto.FactoryMethodName} was not found");
                }

                registration.AsFactoryMethod(ctx => factoryMethod.Invoke(null, new object[] { ctx }));
            }
        }

        private static bool TryGetTagValue(ITypeResolver typeResolver, ITagDto tagDto, out object tagValue)
        {
            if (tagDto == null)
            {
                tagValue = default(object);
                return false;
            }

            Type type;
            if (!typeResolver.TryResolveType(tagDto.TypeName, out type))
            {
                type = typeof(string);
            }

            return TryGetValue(type, tagDto.Value, out tagValue);
        }

        private static bool TryGetValue(ITypeResolver typeResolver, IValueDto valueDto, out object tagValue)
        {
            if (valueDto == null)
            {
                tagValue = default(object);
                return false;
            }

            Type type;
            if (!typeResolver.TryResolveType(valueDto.TypeName, out type))
            {
                type = typeof(string);
            }

            return TryGetValue(type, valueDto.Data, out tagValue);
        }

        private static bool TryGetValue(Type type, string valueText, out object value)
        {
            if (type.GetTypeInfo().IsEnum)
            {
                value = Enum.Parse(type, valueText);
                return true;
            }

            switch (type.Name)
            {
                case nameof(TimeSpan):
                    value = TimeSpan.Parse(valueText, CultureInfo.InvariantCulture);
                    return true;
            }

            value = Convert.ChangeType(valueText, type);
            return true;
        }
    }
}