namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class AutowiringMetadataProvider : IMetadataProvider
    {
        public bool TryResolveImplementationType(IReflection reflection, Type implementationType, out Type resolvedType, ICreationContext creationContext = null)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            var implementationTypeInfo = reflection.GetType(implementationType);
            if (implementationTypeInfo.IsGenericTypeDefinition)
            {
                if (creationContext == null)
                {
                    resolvedType = default(Type);
                    return false;
                }

                var ctx = creationContext.ResolverContext;
                var contractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.FirstOrDefault();
                if (contractKey != null && contractKey.GenericTypeArguments.Length > 0 && implementationTypeInfo.GenericTypeArguments.Length == contractKey.GenericTypeArguments.Length)
                {
                    resolvedType = implementationType.MakeGenericType(contractKey.GenericTypeArguments);
                    return true;
                }
            }

            resolvedType = implementationType;
            return true;
        }

        public bool TrySelectConstructor(IReflection reflection, Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            var implementationTypeInfo = reflection.GetType(implementationType);
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
                    let autowiringAttribute = reflection.GetCustomAttributes<AutowiringAttribute>(ctor, true).FirstOrDefault()
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
                error = new InvalidOperationException("Too many resolving constructors.");
                constructor = default(ConstructorInfo);
                return false;
            }

            error = new InvalidOperationException("Resolving constructor was not found.");
            constructor = default(ConstructorInfo);
            return false;
        }

        public IEnumerable<MethodInfo> GetMethods(IReflection reflection, Type implementationType)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            return
                from method in reflection.GetType(implementationType).Methods
                let autowiringAttribute = reflection.GetCustomAttributes<AutowiringAttribute>(method, true)
                where autowiringAttribute.Any()
                select method;
        }

        public IParameterMetadata[] GetConstructorParameters(IReflection reflection, ConstructorInfo constructor)
        {
#if DEBUG
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            var ctorParams = constructor.GetParameters();
            var arguments = new IParameterMetadata[ctorParams.Length];
            var stateIndex = 0;
            for (var paramIndex = 0; paramIndex < ctorParams.Length; paramIndex++)
            {
                var info = ctorParams[paramIndex];
                var contractAttributes = reflection.GetCustomAttributes<ContractAttribute>(info).ToArray();
                var stateAttributes = reflection.GetCustomAttributes<StateAttribute>(info).OrderBy(i => i.Index).ToArray();
                IStateKey stateKey = null;
                object[] state = null;
                IContractKey[] contractKeys = null;
                ITagKey[] tagKeys = null;
                IStateKey[] stateKeys = null;
                if (stateAttributes.Length == 1 && contractAttributes.Length == 0 && !stateAttributes[0].IsDependency)
                {
                    stateKey = new StateKey(stateIndex, info.ParameterType);
                }
                else
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (contractAttributes.Length > 0)
                    {
                        contractKeys = contractAttributes.SelectMany(i => i.ContractTypes).Select(type => (IContractKey)new ContractKey(reflection, type, true)).DefaultIfEmpty(new ContractKey(reflection, info.ParameterType, true)).ToArray();
                    }
                    else
                    {
                        contractKeys = Enumerable.Repeat((IContractKey)new ContractKey(reflection, info.ParameterType, true), 1).ToArray();
                    }

                    tagKeys = reflection.GetCustomAttributes<TagAttribute>(info).SelectMany(i => i.Tags).Select(i => (ITagKey)new TagKey(i)).ToArray();
                    state = stateAttributes.OrderBy(i => i.Index).Select(i => i.Value).ToArray();
                    stateKeys = stateAttributes.Select(i => (IStateKey)new StateKey(i.Index, i.StateType)).ToArray();
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
