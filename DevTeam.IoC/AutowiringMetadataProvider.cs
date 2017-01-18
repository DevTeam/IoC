namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class AutowiringMetadataProvider : IMetadataProvider
    {
        public static readonly IMetadataProvider Shared = new AutowiringMetadataProvider();

        private AutowiringMetadataProvider()
        {
        }

        public Type ResolveImplementationType(IResolverContext resolverContext, Type type)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (type == null) throw new ArgumentNullException(nameof(type));
            var contractKey = resolverContext.Key.ContractKeys.First();
            if (contractKey != null && contractKey.GenericTypeArguments.Length > 0 && type.GetTypeInfo().GenericTypeParameters.Length == contractKey.GenericTypeArguments.Length)
            {
                return type.MakeGenericType(contractKey.GenericTypeArguments);
            }

            return type;
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            var implementationTypeInfo = implementationType.GetTypeInfo();
            var ctors = implementationTypeInfo.DeclaredConstructors.Where(i => i.IsPublic).ToArray();
            if (ctors.Length == 1)
            {
                constructor = implementationTypeInfo.DeclaredConstructors.First();
                error = default(Exception);
                return true;
            }

            try
            {
                var autowiringConstructor = (
                    from ctor in ctors
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

        public IParameterMetadata[] GetConstructorArguments(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            var ctorParams = constructor.GetParameters();
            var arguments = new IParameterMetadata[ctorParams.Length];
            var stateIndex = 0;
            for (var paramIndex = 0; paramIndex < ctorParams.Length; paramIndex++)
            {
                var paramInfo = new ParameterMetadata(ctorParams[paramIndex], stateIndex);
                if (!paramInfo.IsDependency)
                {
                    stateIndex++;
                }

                arguments[paramIndex] = paramInfo;
            }

            return arguments;
        }

        internal class ParameterMetadata : IParameterMetadata
        {
            public ParameterMetadata(ParameterInfo info, int stateIndex)
            {
                var contractAttributes = info.GetCustomAttributes<ContractAttribute>().ToArray();
                IEnumerable<IContractKey> contractKeys;
                if (contractAttributes.Length > 0)
                {
                    contractKeys = contractAttributes.SelectMany(i => i.ContractTypes).Select(type => (IContractKey)new ContractKey(type, true)).DefaultIfEmpty(new ContractKey(info.ParameterType, true));
                }
                else
                {
                    contractKeys = Enumerable.Empty<IContractKey>();
                }

                var tagKeys = info.GetCustomAttributes<TagAttribute>().SelectMany(i => i.Tags).Select(i => (IKey)new TagKey(i));
                var stateAttributes = info.GetCustomAttributes<StateAttribute>().OrderBy(i => i.Index).ToArray();
                State = stateAttributes.Select(i => i.Value).ToArray();
                var stateKeys = stateAttributes.Select(i => (IKey)new StateKey(i.Index, i.StateType));

                Keys = contractKeys.Concat(tagKeys).Concat(stateKeys).ToArray();
                if (Keys.Any())
                {
                    IsDependency = true;
                }
                else
                {
                    StateKey = new StateKey(stateIndex, info.ParameterType);
                }
            }

            public bool IsDependency { get; }

            public IKey[] Keys { get; }

            public IStateKey StateKey { get; }

            public object[] State { get; }
        }
    }
}
