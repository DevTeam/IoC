namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class ManualMetadataProvider : IMetadataProvider
    {
        private readonly IMetadataProvider _defaultMetadataProvider;
        private readonly IParameterMetadata[] _constructorParams;

        public ManualMetadataProvider(
            [NotNull] IMetadataProvider defaultMetadataProvider,
            [NotNull] IEnumerable<IParameterMetadata> constructorParams)
        {
            if (defaultMetadataProvider == null) throw new ArgumentNullException(nameof(defaultMetadataProvider));
            if (constructorParams == null) throw new ArgumentNullException(nameof(constructorParams));
            _defaultMetadataProvider = defaultMetadataProvider;
            _constructorParams = constructorParams.ToArray();
        }

        public Type ResolveImplementationType(IResolverContext resolverContext, Type implementationType)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            return _defaultMetadataProvider.ResolveImplementationType(resolverContext, implementationType);
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            var typeInfo = implementationType.GetTypeInfo();
            constructor = typeInfo.DeclaredConstructors.Where(MatchConstructor).FirstOrDefault();
            if (constructor == null)
            {
                error = new InvalidOperationException("Constructor was not found.");
                return false;
            }

            error = default(Exception);
            return true;
        }

        public IParameterMetadata[] GetConstructorParameters(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            return _constructorParams;
        }

        private bool MatchConstructor(ConstructorInfo ctor)
        {
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length != _constructorParams.Length)
            {
                return false;
            }

            return ctorParams
                .Zip(_constructorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                .All(i => MatchParameter(i.ctorParam, i.bindingParam));
        }

        private static bool MatchParameter(ParameterInfo paramInfo, IParameterMetadata paramMetadata)
        {
            var parameterTypeInfo = paramInfo.ParameterType.GetTypeInfo();
            TypeInfo genericTypeInfo = null;
            Type[] genericTypeArguments = null;
            if (paramInfo.ParameterType.IsConstructedGenericType)
            {
                genericTypeInfo = parameterTypeInfo.GetGenericTypeDefinition().GetTypeInfo();
                genericTypeArguments = paramInfo.ParameterType.GenericTypeArguments;
            }

            if (paramMetadata.IsDependency)
            {
                return (
                    from contractKey in EnumerateContractKeys(paramMetadata.Keys)
                    let contractTypeInfo = contractKey.ContractType.GetTypeInfo()
                    where parameterTypeInfo.IsAssignableFrom(contractTypeInfo) || (genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(contractTypeInfo) && contractKey.GenericTypeArguments.SequenceEqual(genericTypeArguments))
                    select contractKey).Any();
            }

            if (paramMetadata.StateKey == null)
            {
                return true;
            }

            var stateTypeInfo = paramMetadata.StateKey.StateType.GetTypeInfo();
            return parameterTypeInfo.IsAssignableFrom(stateTypeInfo) || (genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(stateTypeInfo));
        }

        private static IEnumerable<IContractKey> EnumerateContractKeys(IEnumerable<IKey> keys)
        {
            foreach (var key in keys)
            {
                var contractKey = key as IContractKey;
                if (contractKey != null)
                {
                    yield return contractKey;
                }

                var compositeKey = key as ICompositeKey;
                if (compositeKey != null)
                {
                    foreach (var subContractKey in compositeKey.ContractKeys)
                    {
                        yield return subContractKey;
                    }
                }
            }
        }
    }
}