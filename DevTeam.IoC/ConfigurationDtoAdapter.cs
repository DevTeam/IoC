namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
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

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var typeResolver = container.Resolve().Instance<ITypeResolver>();
            var reflection = container.Resolve().Instance<IReflection>();
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
                    yield return container.Feature(dependencyFeatureDto.Feature);
                    continue;
                }

                var dependencyConfigurationDto = configurationStatement as IDependencyConfigurationDto;
                if (dependencyConfigurationDto != null)
                {
                    Type configurationType;
                    if (!typeResolver.TryResolveType(dependencyConfigurationDto.ConfigurationTypeName, out configurationType) || !reflection.GetType(typeof(IConfiguration)).IsAssignableFrom(reflection.GetType(configurationType)))
                    {
                        throw new Exception($"Invalid configuration type {configurationType}");
                    }

                    using (
                        var childContainer = container.CreateChild()
                        .Register().Contract<IConfiguration>().Autowiring(configurationType).ToSelf())
                    {
                        yield return childContainer.Resolve().Instance<IConfiguration>();
                    }

                    continue;
                }

                var dependencyAssemblyDto = configurationStatement as IDependencyAssemblyDto;
                if (dependencyAssemblyDto != null)
                {
                    var assembly = Assembly.Load(new AssemblyName(dependencyAssemblyDto.AssemblyName));
                    yield return container.Resolve().State<Assembly>(0).Instance<IConfiguration>(assembly);
                    continue;
                }

                var dependencyReferenceDto = configurationStatement as IDependencyReferenceDto;
                if (dependencyReferenceDto != null)
                {
                    Type configurationType;
                    if (!typeResolver.TryResolveType(dependencyReferenceDto.ConfigurationTypeName, out configurationType) || !reflection.GetType(typeof(IConfiguration)).IsAssignableFrom(reflection.GetType(configurationType)))
                    {
                        throw new Exception($"Invalid configuration type {configurationType}");
                    }

                    using (var childContainer = container.CreateChild())
                    {
                        var referenceDescriptionResolver = childContainer.Resolve().Instance<IReferenceDescriptionResolver>();
                        var reference = referenceDescriptionResolver.ResolveReference(dependencyReferenceDto.Reference);
                        var configurationDescriptionDto = childContainer.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(reference);
                        var configurationDto = childContainer.Resolve().Tag(configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
                        yield return new ConfigurationDtoAdapter(configurationDto);
                    }
                }
            }
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();
            return Apply(container, reflection, container.Resolve().Instance<ITypeResolver>(), _configurationDto);
        }

        private IEnumerable<IDisposable> Apply([NotNull] IContainer container, [NotNull] IReflection reflection, [NotNull] ITypeResolver typeResolver, [NotNull] IEnumerable<IConfigurationStatementDto> configurationElements)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (configurationElements == null) throw new ArgumentNullException(nameof(configurationElements));
            foreach (var configurationStatement in configurationElements)
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
                        if (!TryGetTagValue(reflection, typeResolver, containerDto.Tag, out containerTag))
                        {
                            throw new Exception($"Invalid container tag {containerDto.Tag.Value}");
                        }
                    }

                    foreach(var registration in Apply(container.CreateChild(containerTag), reflection, typeResolver, containerDto.Statements))
                    {
                        yield return registration;
                    }

                    continue;
                }

                var registerDto = configurationStatement as IRegisterDto;
                if (registerDto != null)
                {
                    foreach (var registration in HandleRegisterDto(container, reflection, typeResolver, registerDto))
                    {
                        yield return registration;
                    }
                }
            }
        }

        private IEnumerable<IDisposable> HandleRegisterDto([NotNull] IContainer container, [NotNull] IReflection reflection, [NotNull] ITypeResolver typeResolver, [NotNull] IRegisterDto registerDto)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (registerDto == null) throw new ArgumentNullException(nameof(registerDto));
            var registration = container.Register();
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
                    if (!TryGetTagValue(reflection, typeResolver, tagDto, out tag))
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

                        Resolving<IContainer> resolving = null;
                        IStateKey stateKey = null;
                        object value = null;
                        var state = new List<object>();
                        if (ctorParam.Value != null)
                        {
                            if (!TryGetValue(reflection, parameterType, ctorParam.Value, out value))
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
                                    if (!TryGetValue(reflection, typeResolver, ctorParam.State.Value, out value))
                                    {
                                        throw new Exception($"Invalid state {ctorParam.State.Value.Data}");
                                    }

                                    state.Add(value);
                                }

                                stateKey = new StateKey(reflection, ctorParam.State.Index, stateType, true);
                            }
                        }
                        else
                        {
                            if (ctorParam.Dependency != null)
                            {
                                resolving = new Resolving<IContainer>(container);
                                var hasContractKey = false;
                                foreach (var keyDto in ctorParam.Dependency)
                                {
                                    if (keyDto is IContractDto contractDto)
                                    {
                                        var contractTypes = new List<Type>();
                                        foreach (var typeName in contractDto.Contract)
                                        {
                                            if (!typeResolver.TryResolveType(typeName, out Type contractType))
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
                                            if (!TryGetValue(reflection, typeResolver, stateDto.Value, out stetItem))
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
                                        if (!TryGetTagValue(reflection, typeResolver, tagDto, out tag))
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
                            }
                        }

                        var param = new ParameterMetadata(
                            resolving?.ContractKeys.ToArray() ?? new IContractKey[] { new ContractKey(reflection, parameterType, true) },
                            resolving?.TagKeys?.ToArray(),
                            resolving?.StateKeys?.ToArray(),
                            stateIndex,
                            state.ToArray(),
                            value,
                            stateKey);

                        constructorParameters.Add(param);
                        if (!param.IsDependency)
                        {
                            stateIndex++;
                        }
                    }

                    metadataProvider = container.Resolve().State<IEnumerable<IParameterMetadata>>(0).Instance<IMetadataProvider>(constructorParameters);
                }

                yield return registration.Autowiring(autowiringType, false, metadataProvider).Apply();
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
                if (!typeResolver.TryResolveType(factoryMethodTypeName, out factoryMethodType))
                {
                    throw new Exception($"Invalid factory method type {factoryMethodName}");
                }

                var factoryMethod = reflection.GetType(factoryMethodType).GetMethod(factoryMethodName, new[] { typeof(ICreationContext) });
                if (factoryMethod == null)
                {
                    throw new Exception($"Factory method {registerDto.FactoryMethodName} was not found");
                }

                yield return registration.FactoryMethod(ctx => factoryMethod.Invoke(null, new object[] { ctx })).Apply();
            }
        }

        private static bool TryGetTagValue([NotNull] IReflection reflection, [NotNull] ITypeResolver typeResolver, [CanBeNull] ITagDto tagDto, out object tagValue)
        {
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
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

            return TryGetValue(reflection, type, tagDto.Value, out tagValue);
        }

        private static bool TryGetValue([NotNull] IReflection reflection, [NotNull] ITypeResolver typeResolver, [CanBeNull] IValueDto valueDto, out object tagValue)
        {
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
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

            return TryGetValue(reflection, type, valueDto.Data, out tagValue);
        }

        private static bool TryGetValue([NotNull] IReflection reflection, [NotNull] Type type, string valueText, out object value)
        {
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (reflection.GetType(type).IsEnum)
            {
                value = Enum.Parse(type, valueText);
                return true;
            }

            switch (type.Name)
            {
                case nameof(TimeSpan):
                    value = TimeSpan.Parse(valueText);
                    return true;
            }

            value = Convert.ChangeType(valueText, type);
            return true;
        }
    }
}