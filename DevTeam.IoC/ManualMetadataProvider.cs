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
#if DEBUG
            if (defaultMetadataProvider == null) throw new ArgumentNullException(nameof(defaultMetadataProvider));
            if (constructorParams == null) throw new ArgumentNullException(nameof(constructorParams));
#endif
            _defaultMetadataProvider = defaultMetadataProvider;
            _constructorParams = constructorParams.ToArray();
        }

        public bool TryResolveImplementationType(Type implementationType, out Type resolvedType, ICreationContext creationContext = null)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            return _defaultMetadataProvider.TryResolveImplementationType(implementationType, out resolvedType, creationContext);
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
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
#if DEBUG
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
#endif
            return _constructorParams;
        }

        private bool MatchConstructor([NotNull] ConstructorInfo ctor)
        {
#if DEBUG
            if (ctor == null) throw new ArgumentNullException(nameof(ctor));
#endif
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length != _constructorParams.Length)
            {
                return false;
            }

            return ctorParams
                .Zip(_constructorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                .All(i => MatchParameter(i.ctorParam, i.bindingParam));
        }

        private static bool MatchParameter([NotNull] ParameterInfo paramInfo, [NotNull] IParameterMetadata paramMetadata)
        {
#if DEBUG
            if (paramInfo == null) throw new ArgumentNullException(nameof(paramInfo));
            if (paramMetadata == null) throw new ArgumentNullException(nameof(paramMetadata));
#endif
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
                    from contractKey in paramMetadata.ContractKeys ?? Enumerable.Empty<IContractKey>()
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
    }
}