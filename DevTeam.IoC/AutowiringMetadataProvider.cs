namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class AutowiringMetadataProvider : IMetadataProvider
    {
        public Type ResolveImplementationType(IResolverContext resolverContext, Type implementationType)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            var contractKey = resolverContext.Key.ContractKeys.FirstOrDefault();
            if (contractKey != null && contractKey.GenericTypeArguments.Length > 0 && implementationType.GetTypeInfo().GenericTypeParameters.Length == contractKey.GenericTypeArguments.Length)
            {
                return implementationType.MakeGenericType(contractKey.GenericTypeArguments);
            }

            return implementationType;
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            var implementationTypeInfo = implementationType.GetTypeInfo();
            var constructorInfos = implementationTypeInfo.DeclaredConstructors.Where(i => i.IsPublic).ToArray();
            if (constructorInfos.Length == 1)
            {
                constructor = implementationTypeInfo.DeclaredConstructors.First();
                error = default(Exception);
                return true;
            }

            try
            {
                var autowiringConstructor = (
                    from ctor in constructorInfos
                    let autowiringAttribute = ctor.GetCustomAttribute<AutowiringAttribute>()
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

        public IParameterMetadata[] GetConstructorParameters(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            var ctorParams = constructor.GetParameters();
            var arguments = new IParameterMetadata[ctorParams.Length];
            var stateIndex = 0;
            for (var paramIndex = 0; paramIndex < ctorParams.Length; paramIndex++)
            {
                var info = ctorParams[paramIndex];
                var contractAttributes = info.GetCustomAttributes<ContractAttribute>().ToArray();
                var stateAttributes = info.GetCustomAttributes<StateAttribute>().OrderBy(i => i.Index).ToArray();
                IStateKey stateKey = null;
                IKey[] keys = null;
                object[] state = null;
                if (stateAttributes.Length == 1 && contractAttributes.Length == 0 && !stateAttributes[0].IsDependency)
                {
                    stateKey = new StateKey(stateIndex, info.ParameterType);
                }
                else
                {
                    IEnumerable<IContractKey> contractKeys;
                    if (contractAttributes.Length > 0)
                    {
                        contractKeys = contractAttributes.SelectMany(i => i.ContractTypes).Select(type => (IContractKey)new ContractKey(type, true)).DefaultIfEmpty(new ContractKey(info.ParameterType, true));
                    }
                    else
                    {
                        contractKeys = Enumerable.Repeat((IContractKey)new ContractKey(info.ParameterType, true), 1);
                    }

                    var tagKeys = info.GetCustomAttributes<TagAttribute>().SelectMany(i => i.Tags).Select(i => (IKey)new TagKey(i));
                    state = stateAttributes.OrderBy(i => i.Index).Select(i => i.Value).ToArray();
                    var stateKeys = stateAttributes.Select(i => (IKey)new StateKey(i.Index, i.StateType));
                    keys = contractKeys.Concat(tagKeys).Concat(stateKeys).ToArray();
                }

                var paramInfo = new ParameterMetadata(
                    constructor.DeclaringType,
                    keys,
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
