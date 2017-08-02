namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class AutowiringMetadataProvider : IMetadataProvider
    {
        [NotNull] private readonly IReflection _reflection;
        [NotNull] private readonly Cache<Type, Cache<IContractKey, Type>> _resolvedTypes = new Cache<Type, Cache<IContractKey, Type>>();

        public AutowiringMetadataProvider([NotNull] IReflection reflection)
        {
            _reflection = reflection ?? throw new ArgumentNullException(nameof(reflection));
        }

        public bool TryResolveType(Type implementationType, out Type resolvedType, ICreationContext creationContext = null)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            var typeInfo = _reflection.GetType(implementationType);
            if (!typeInfo.IsGenericTypeDefinition)
            {
                resolvedType = implementationType;
                return true;
            }

            if (creationContext == null)
            {
                resolvedType = default(Type);
                return false;
            }

            var ctx = creationContext.ResolverContext;
            var key = ctx.Key;
            var contractKey = key as IContractKey ?? (key as ICompositeKey)?.ContractKeys.FirstOrDefault();
            if (contractKey != null)
            {
                if (!_resolvedTypes.TryGet(implementationType, out Cache<IContractKey, Type> types))
                {
                    types = new Cache<IContractKey, Type>();
                    _resolvedTypes.Set(implementationType, types);
                }

                if (types.TryGet(contractKey, out resolvedType))
                {
                    return true;
                }

                if (contractKey.GenericTypeArguments.Length > 0 && typeInfo.GenericTypeParameters.Length == contractKey.GenericTypeArguments.Length)
                {
                    resolvedType = implementationType.MakeGenericType(contractKey.GenericTypeArguments);
                    types.Set(contractKey, resolvedType);
                    return true;
                }
            }

            resolvedType = default(Type);
            return false;
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            var implementationTypeInfo = _reflection.GetType(implementationType);
            var constructorInfos = implementationTypeInfo.Constructors.Where(i => i.IsPublic).ToArray();
            if (constructorInfos.Length == 1)
            {
                constructor = implementationTypeInfo.Constructors.First();
                error = default(Exception);
                return true;
            }

            constructorInfos = implementationTypeInfo.Constructors.ToArray();
            if (constructorInfos.Length == 1)
            {
                constructor = implementationTypeInfo.Constructors.First();
                error = default(Exception);
                return true;
            }

            try
            {
                var autowiringConstructor = (
                    from ctor in constructorInfos
                    let autowiringAttribute = _reflection.GetCustomAttributes<AutowiringAttribute>(ctor, true).FirstOrDefault()
                    where autowiringAttribute != null
                    select ctor).SingleOrDefault();

                if (autowiringConstructor != null)
                {
                    constructor = autowiringConstructor;
                    error = default(Exception);
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                error = new ContainerException($"Too many public resolving constructors in the type \"{implementationType}\".");
                constructor = default(ConstructorInfo);
                return false;
            }

            error = new ContainerException($"Any public resolving constructor was not found in the type \"{implementationType}\".");
            constructor = default(ConstructorInfo);
            return false;
        }

        public IEnumerable<MethodInfo> GetMethods(Type implementationType)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            return
                from method in _reflection.GetType(implementationType).Methods
                let autowiringAttribute = _reflection.GetCustomAttributes<AutowiringAttribute>(method, true).FirstOrDefault()
                where autowiringAttribute != null
                orderby autowiringAttribute.Order
                select method;
        }

        public IParameterMetadata[] GetParameters(MethodBase method, ref int stateIndex)
        {
#if DEBUG
            if (method == null) throw new ArgumentNullException(nameof(method));
#endif
            var ctorParams = method.GetParameters();
            var arguments = new IParameterMetadata[ctorParams.Length];
            for (var paramIndex = 0; paramIndex < ctorParams.Length; paramIndex++)
            {
                var info = ctorParams[paramIndex];
                var contractAttributes = _reflection.GetCustomAttributes<ContractAttribute>(info).ToArray();
                var stateAttributes = _reflection.GetCustomAttributes<StateAttribute>(info).OrderBy(i => i.Index).ToArray();
                IStateKey stateKey = null;
                object[] state = null;
                IContractKey[] contractKeys = null;
                ITagKey[] tagKeys = null;
                IStateKey[] stateKeys = null;
                if (stateAttributes.Length == 1 && contractAttributes.Length == 0 && !stateAttributes[0].IsDependency)
                {
                    stateKey = new StateKey(_reflection, stateIndex, info.ParameterType, true);
                }
                else
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (contractAttributes.Length > 0)
                    {
                        contractKeys = contractAttributes.SelectMany(i => i.ContractTypes).Select(type => (IContractKey)new ContractKey(_reflection, type, true)).DefaultIfEmpty(new ContractKey(_reflection, info.ParameterType, true)).ToArray();
                    }
                    else
                    {
                        contractKeys = Enumerable.Repeat((IContractKey)new ContractKey(_reflection, info.ParameterType, true), 1).ToArray();
                    }

                    tagKeys = _reflection.GetCustomAttributes<TagAttribute>(info).SelectMany(i => i.Tags).Select(i => (ITagKey)new TagKey(i)).ToArray();
                    state = stateAttributes.OrderBy(i => i.Index).Select(i => i.Value).ToArray();
                    stateKeys = stateAttributes.Select(i => (IStateKey)new StateKey(_reflection, i.Index, i.StateType, true)).ToArray();
                    contractKeys = contractKeys.Length > 0 ? contractKeys : null;
                    tagKeys = tagKeys.Length > 0 ? tagKeys : null;
                    stateKeys = stateKeys.Length > 0 ? stateKeys : null;
                }

                var paramInfo = new ParameterMetadata(
                    contractKeys,
                    tagKeys,
                    stateKeys,
                    stateIndex,
                    state,
                    null,
                    stateKey);

                if (!paramInfo.IsDependency)
                {
                    stateIndex++;
                }

                arguments[paramIndex] = paramInfo;
            }

            return arguments;
        }
    }
}
