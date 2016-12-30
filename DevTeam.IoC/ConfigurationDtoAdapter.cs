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

        private static void HandleApplyStatementDto(IResolver resolver, ITypeResolver typeResolver, IApplyStatementDto applyDtoStatement)
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

        private static void HandleRegisterDto(IResolver resolver, ITypeResolver typeResolver, IRegisterDto registerDto)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (registerDto == null) throw new ArgumentNullException(nameof(registerDto));
            var registration = resolver.Register();
            foreach (var registerStatementDto in registerDto.Statements)
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
                    if (!typeResolver.TryResolveType(stateDto.TypeName, out stateType))
                    {
                        throw new Exception($"Invalid state type {stateDto.TypeName}");
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

                registration.AsAutowiring(autowiringType);
                return;
            }

            if (!string.IsNullOrWhiteSpace(registerDto.FactoryMethodName))
            {
                var parts = registerDto.FactoryMethodName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
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

                var factoryMethod = factoryMethodType.GetRuntimeMethod(factoryMethodName,
                    new[] {typeof(IResolverContext)});
                if (factoryMethod == null)
                {
                    throw new Exception($"Factory method {registerDto.FactoryMethodName} was not found");
                }

                registration.AsFactoryMethod(ctx => factoryMethod.Invoke(null, new object[] {ctx}));
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
            if(!typeResolver.TryResolveType(tagDto.TypeName, out  type))
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
    }
}