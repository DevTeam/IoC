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

        public bool TryResolveImplementationType(IReflection reflection, Type implementationType, out Type resolvedType, ICreationContext creationContext = null)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            return _defaultMetadataProvider.TryResolveImplementationType(reflection, implementationType, out resolvedType, creationContext);
        }

        public bool TrySelectConstructor(IReflection reflection, Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            var typeInfo = reflection.GetType(implementationType);
            constructor = typeInfo.Constructors.FirstOrDefault(ctor => MatchConstructor(reflection, ctor));
            if (constructor == null)
            {
                error = new InvalidOperationException("Constructor was not found.");
                return false;
            }

            error = default(Exception);
            return true;
        }

        public IEnumerable<MethodInfo> GetMethods(IReflection reflection, Type implementationType)
        {
            yield break;
        }

        public IParameterMetadata[] GetParameters(IReflection reflection, MethodBase method, ref int stateIndex)
        {
#if DEBUG
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            return _constructorParams;
        }

        private bool MatchConstructor([NotNull] IReflection reflection, [NotNull] ConstructorInfo ctor)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (ctor == null) throw new ArgumentNullException(nameof(ctor));
#endif
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length != _constructorParams.Length)
            {
                return false;
            }

            return ctorParams
                .Zip(_constructorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                .All(i => MatchParameter(reflection, i.ctorParam, i.bindingParam));
        }

        private static bool MatchParameter([NotNull] IReflection reflection, [NotNull] ParameterInfo paramInfo, [NotNull] IParameterMetadata paramMetadata)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (paramInfo == null) throw new ArgumentNullException(nameof(paramInfo));
            if (paramMetadata == null) throw new ArgumentNullException(nameof(paramMetadata));
#endif
            var parameterTypeInfo = reflection.GetType(paramInfo.ParameterType);
            IType genericTypeInfo = null;
            Type[] genericTypeArguments = null;
            if (reflection.GetType(paramInfo.ParameterType).IsConstructedGenericType)
            {
                var genericTypeDefinition = parameterTypeInfo.GenericTypeDefinition;
                if (genericTypeDefinition != null)
                {
                    genericTypeInfo = reflection.GetType(genericTypeDefinition);
                    genericTypeArguments = reflection.GetType(paramInfo.ParameterType).GenericTypeArguments;
                }
            }

            if (paramMetadata.IsDependency)
            {
                return (
                    from contractKey in paramMetadata.ContractKeys ?? Enumerable.Empty<IContractKey>()
                    let contractTypeInfo = reflection.GetType(contractKey.ContractType)
                    where parameterTypeInfo.IsAssignableFrom(contractTypeInfo) || genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(contractTypeInfo) && genericTypeArguments != null && contractKey.GenericTypeArguments.SequenceEqual(genericTypeArguments)
                    select contractKey).Any();
            }

            if (paramMetadata.StateKey == null)
            {
                return true;
            }

            var stateTypeInfo = reflection.GetType(paramMetadata.StateKey.StateType);
            return parameterTypeInfo.IsAssignableFrom(stateTypeInfo) || genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(stateTypeInfo);
        }
    }
}