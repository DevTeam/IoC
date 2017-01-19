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
        private readonly IKeyFactory _keyFactory;

        public ConfigurationDtoAdapter(
            IConfigurationDto configurationDto)
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

                var usingStatementDto = configurationStatement as IUsingDto;
                if (usingStatementDto != null)
                {
                    typeResolver.AddUsing(usingStatementDto.Using);
                    continue;
                }

                var getDependeciesDto = configurationStatement as IGetDependeciesDto;
                if (getDependeciesDto != null)
                {
                    foreach (var dependencyDto in getDependeciesDto.Dependecies)
                    {
                        var wellknownConfigurationDto = dependencyDto as IWellknownConfigurationDto;
                        if (wellknownConfigurationDto != null)
                        {
                            yield return resolver.Configuration(wellknownConfigurationDto.Configuration);
                            continue;
                        }

                        var configurationTypeDto = dependencyDto as IConfigurationTypeDto;
                        if (configurationTypeDto != null)
                        {
                            Type configurationType;
                            if (!typeResolver.TryResolveType(configurationTypeDto.ConfigurationTypeName, out configurationType) || !typeof(IConfiguration).GetTypeInfo().IsAssignableFrom(configurationType.GetTypeInfo()))
                            {
                                throw new Exception($"Invalid configuration type {configurationType}");
                            }

                            var childContainer = resolver.CreateChild();
                            childContainer.Register().Contract<IConfiguration>().AsAutowiring(configurationType);
                            yield return childContainer.Resolve().Instance<IConfiguration>();
                            continue;
                        }

                        var configurationReferenceDto = dependencyDto as IConfigurationReferenceDto;
                        if (configurationReferenceDto != null)
                        {
                            Type configurationType;
                            if (!typeResolver.TryResolveType(configurationReferenceDto.ConfigurationTypeName, out configurationType) || !typeof(IConfiguration).GetTypeInfo().IsAssignableFrom(configurationType.GetTypeInfo()))
                            {
                                throw new Exception($"Invalid configuration type {configurationType}");
                            }

                            var childContainer = resolver.CreateChild();
                            var referenceDescriptionResolver = childContainer.Resolve().Instance<IReferenceDescriptionResolver>();
                            var reference = referenceDescriptionResolver.ResolveReference(configurationReferenceDto.Reference);
                            var configurationDescriptionDto = childContainer.Resolve().State<string>(0).Instance<IConfigurationDescriptionDto>(reference);
                            var сonfigurationDto = childContainer.Resolve().Tag(configurationType).State<IConfigurationDescriptionDto>(0).Instance<IConfigurationDto>(configurationDescriptionDto);
                            yield return new ConfigurationDtoAdapter(сonfigurationDto);
                            continue;
                        }
                    }
                    continue;
                }
            }
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
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

                var usingStatementDto = configurationStatement as IUsingDto;
                if (usingStatementDto != null)
                {
                    typeResolver.AddUsing(usingStatementDto.Using);
                    continue;
                }

                var applyDto = configurationStatement as IApplyDto;
                if (applyDto != null)
                {
                    foreach (var applyDtoStatement in applyDto.Statements)
                    {
                        HandleApplyStatementDto(resolver, typeResolver, applyDtoStatement);
                    }
                }
            }

            yield break;
        }

        private void HandleApplyStatementDto(IResolver resolver, ITypeResolver typeResolver, IApplyStatementDto applyDtoStatement)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (applyDtoStatement == null) throw new ArgumentNullException(nameof(applyDtoStatement));
            {
                var createChildDto = applyDtoStatement as ICreateChildDto;
                if (createChildDto != null)
                {
                    var childContainer = resolver.CreateChild();
                    foreach (var applyStatementDto in createChildDto.Statements)
                    {
                        HandleApplyStatementDto(childContainer, typeResolver, applyStatementDto);
                    }

                    return;
                }

                var registerDto = applyDtoStatement as IRegisterDto;
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
            foreach (var keyDto in registerDto.Keys)
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

                    registration.Contract(contractTypes.ToArray());
                    continue;
                }

                var stateDto = keyDto as IStateDto;
                if (stateDto != null)
                {
                    Type stateType;
                    if (!typeResolver.TryResolveType(stateDto.TypeName, out stateType))
                    {
                        throw new Exception($"Invalid state type {stateDto.TypeName}");
                    }

                    registration.State(stateDto.Index, stateType);
                    continue;
                }

                var tagDto = keyDto as ITagDto;
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

                var lifetimeDto = keyDto as ILifetimeDto;
                if (lifetimeDto != null)
                {
                    registration.Lifetime(lifetimeDto.Lifetime);
                    continue;
                }

                var scopeDto = keyDto as IScopeDto;
                if (scopeDto != null)
                {
                    registration.Scope(scopeDto.Scope);
                    continue;
                }

                var keyComparerDto = keyDto as IKeyComparerDto;
                if (keyComparerDto != null)
                {
                    registration.KeyComparer(keyComparerDto.KeyComparer);
                    continue;
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
                if (registerDto.Binding != null)
                {
                    var constructorParameters = new List<ConstructorParameter>();
                    var bindingCtorParams = registerDto.Binding.ConstructorParameters.ToArray();
                    int stateIndex = 0;
                    foreach (var ctorParam in bindingCtorParams)
                    {
                        Type type;
                        if (!typeResolver.TryResolveType(ctorParam.TypeName, out type))
                        {
                            throw new Exception($"Invalid constructor parameter type {ctorParam.TypeName}");
                        }

                        IKey key = null;
                        if (ctorParam.Keys != null)
                        {
                            var resolving = new Resolving(resolver.Resolve().Instance<IFluent>(), resolver);
                            foreach (var keyDto in ctorParam.Keys)
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

                                    resolving.Contract(contractTypes.ToArray());
                                }

                                var stateDto = keyDto as IStateDto;
                                if (stateDto != null)
                                {
                                    Type stateType;
                                    if (!typeResolver.TryResolveType(stateDto.TypeName, out stateType))
                                    {
                                        throw new Exception($"Invalid state type {stateDto.TypeName}");
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

                            key = resolving.CreateCompositeKey();
                        }

                        var isDependency = key != null;
                        constructorParameters.Add(new ConstructorParameter(type, new []{ key }, isDependency, stateIndex));

                        if (!isDependency)
                        {
                            stateIndex++;
                        }
                    }

                    metadataProvider = new MetadataProvider(constructorParameters);
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

        //private IKey CreateKey(IResolver resolver, ITypeResolver typeResolver, IKeyDto keyDto)
        //{
        //    var resolving = new Resolving(resolver.Resolve().Instance<IFluent>(), resolver);

        //    var contractDto = keyDto as IContractDto;
        //    if (contractDto != null)
        //    {
        //        var contractTypes = new List<Type>();
        //        foreach (var typeName in contractDto.Contract)
        //        {
        //            Type contractType;
        //            if (!typeResolver.TryResolveType(typeName, out contractType))
        //            {
        //                throw new Exception($"Invalid contract type {typeName}");
        //            }

        //            contractTypes.Add(contractType);
        //        }

        //        resolving.Contract()


        //        return _keyFactory.CreateCompositeKey(contractKeys.ToArray(), new ITagKey[0], new IStateKey[0]);
        //    }

        //    var stateDto = keyDto as IStateDto;
        //    if (stateDto != null)
        //    {
        //        Type stateType;
        //        if (!typeResolver.TryResolveType(stateDto.TypeName, out stateType))
        //        {
        //            throw new Exception($"Invalid state type {stateDto.TypeName}");
        //        }

        //        return _keyFactory.CreateStateKey(stateDto.Index, stateType);
        //    }

        //    var tagDto = keyDto as ITagDto;
        //    if (tagDto != null)
        //    {
        //        object tag;
        //        if (!TryGetTagValue(typeResolver, tagDto, out tag))
        //        {
        //            throw new Exception($"Invalid tag {tagDto.Value}");
        //        }

        //        return _keyFactory.CreateTagKey(tag);
        //    }

        //    throw new Exception($"Invalid typ of key {keyDto.GetType()}");
        //}

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

        private static bool TryGetValue(Type type, string valueText, out object value)
        {
            if (type.GetTypeInfo().IsEnum)
            {
                value = Enum.Parse(type, valueText);
                return true;
            }

            value = Convert.ChangeType(valueText, type);
            return true;
        }

        private class ConstructorParameter : IParameterMetadata
        {
            public ConstructorParameter(Type type, IKey[] keys, bool isDependency, int stateIndex)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                if (keys == null) throw new ArgumentNullException(nameof(keys));
                Type = type;
                Keys = keys;
                IsDependency = isDependency;
                if (!isDependency)
                {
                    StateKey = new StateKey(stateIndex, type);
                }
            }

            public Type Type { get; }

            public bool IsDependency { get; }

            public object[] State { get; }

            public IStateKey StateKey { get; }

            public IKey[] Keys { get; }

        }

        private class MetadataProvider : IMetadataProvider
        {
            private readonly ICollection<ConstructorParameter> _bindingCtorParams;
            private readonly IParameterMetadata[] _constructorArguments;

            public MetadataProvider(ICollection<ConstructorParameter> bindingCtorParams)
            {
                if (bindingCtorParams == null) throw new ArgumentNullException(nameof(bindingCtorParams));
                _bindingCtorParams = bindingCtorParams;
                _constructorArguments = bindingCtorParams.Cast<IParameterMetadata>().ToArray();
            }

            public Type ResolveImplementationType(IResolverContext resolverContext, Type type)
            {
                return AutowiringMetadataProvider.Shared.ResolveImplementationType(resolverContext, type);
            }

            public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
            {
                var typeInfo = implementationType.GetTypeInfo();
                constructor = typeInfo.DeclaredConstructors.Where(MatchConstructor).FirstOrDefault();
                error = default(Exception);
                return constructor != null;
            }

            public IParameterMetadata[] GetConstructorParameters(ConstructorInfo constructor)
            {
                return _constructorArguments;
            }

            private bool MatchConstructor(ConstructorInfo ctor)
            {
                var ctorParams = ctor.GetParameters();
                if (ctorParams.Length != _bindingCtorParams.Count)
                {
                    return false;
                }

                return ctorParams
                    .Zip(_bindingCtorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                    .Any(i => MatchParameter(i.ctorParam, i.bindingParam));
            }

            private static bool MatchParameter(ParameterInfo ctorParam, ConstructorParameter bindingCtorParam)
            {
                return ctorParam.ParameterType == bindingCtorParam.Type;
            }
        }
    }
}