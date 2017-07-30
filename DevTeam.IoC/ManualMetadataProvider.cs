namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class ManualMetadataProvider : IMetadataProvider
    {
        private readonly IMetadataProvider _defaultMetadataProvider;
        [NotNull] private readonly IReflection _reflection;
        [CanBeNull] private readonly IParameterMetadata[] _constructorParams;
        [CanBeNull] private MethodMetadata[] _methods;
        [CanBeNull] private PropertyMetadata[] _properties;
        [CanBeNull] private IDictionary<MethodInfo, IParameterMetadata[]> _methodsDict;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public ManualMetadataProvider(
            [NotNull] IMetadataProvider defaultMetadataProvider,
            [NotNull] IReflection reflection,
            [NotNull] TypeMetadata typeMetadata)
        {
#if DEBUG
            if (defaultMetadataProvider == null) throw new ArgumentNullException(nameof(defaultMetadataProvider));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
            if (typeMetadata == null) throw new ArgumentNullException(nameof(typeMetadata));
#endif
            _defaultMetadataProvider = defaultMetadataProvider;
            _reflection = reflection;
            _constructorParams = typeMetadata.Constructor?.Parameters.ToArray();
            _methods = typeMetadata.Methods?.ToArray();
            _properties = typeMetadata.Properties?.ToArray();
        }

        public bool TryResolveType(Type implementationType, out Type resolvedType, ICreationContext creationContext = null)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            return _defaultMetadataProvider.TryResolveType(implementationType, out resolvedType, creationContext);
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            if (_constructorParams == null)
            {
                return _defaultMetadataProvider.TrySelectConstructor(implementationType, out constructor, out error);
            }

            var typeInfo = _reflection.GetType(implementationType);
            constructor = typeInfo.Constructors.FirstOrDefault(MatchMethod);
            if (constructor == null)
            {
                return _defaultMetadataProvider.TrySelectConstructor(implementationType, out constructor, out error);
            }

            error = default(Exception);
            return true;
        }

        public IEnumerable<MethodInfo> GetMethods(Type implementationType)
        {
            return GetMethodsInternal(implementationType).Keys;
        }

        public IParameterMetadata[] GetParameters(MethodBase method, ref int stateIndex)
        {
#if DEBUG
            if (method == null) throw new ArgumentNullException(nameof(method));
#endif
            if (method is MethodInfo methodInfo && method.DeclaringType != null)
            {
                var methodsDict = GetMethodsInternal(method.DeclaringType);
                if (methodsDict.TryGetValue(methodInfo, out IParameterMetadata[] pars))
                {
                    return pars;
                }
            }

            return _constructorParams ?? _defaultMetadataProvider.GetParameters(method, ref stateIndex);
        }

        private IDictionary<MethodInfo, IParameterMetadata[]> GetMethodsInternal([NotNull] Type implementationType)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
#endif
            if (_methodsDict != null)
            {
                return _methodsDict;
            }

            _methodsDict = new Dictionary<MethodInfo, IParameterMetadata[]>();
            var type = _reflection.GetType(implementationType);
            var methods = type.Methods.ToArray();

            if (_methods != null)
            {
                var methodList =
                    from methodMetadata in _methods
                    join method in methods on methodMetadata.Name equals method.Name
                    where
                        method.GetParameters().Zip(methodMetadata.Parameters, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                        .All(i => MatchParameter(i.ctorParam, i.bindingParam))
                    select new { method, methodMetadata.Parameters };

                foreach (var item in methodList)
                {
                    _methodsDict.Add(item.method, item.Parameters.ToArray());
                }
            }

            if (_properties != null)
            {
                var methodList =
                    from method in methods
                    let pars = method.GetParameters()
                    where pars.Length == 1
                    join propertyMetadata in _properties on method.Name equals "set_" + propertyMetadata.Name
                    where MatchParameter(pars[0], propertyMetadata.Parameter)
                    select new { method, propertyMetadata.Parameter };

                foreach (var item in methodList)
                {
                    _methodsDict.Add(item.method, new[] { item.Parameter });
                }
            }

            return _methodsDict;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool MatchMethod([NotNull] MethodBase method)
        {
#if DEBUG
            if (method == null) throw new ArgumentNullException(nameof(method));
#endif
            if (_constructorParams == null)
            {
                return false;
            }

            var ctorParams = method.GetParameters();
            if (ctorParams.Length != _constructorParams.Length)
            {
                return false;
            }

            return ctorParams
                .Zip(_constructorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                .All(i => MatchParameter(i.ctorParam, i.bindingParam));
        }

        private bool MatchParameter([NotNull] ParameterInfo paramInfo, [NotNull] IParameterMetadata paramMetadata)
        {
#if DEBUG
            if (paramInfo == null) throw new ArgumentNullException(nameof(paramInfo));
            if (paramMetadata == null) throw new ArgumentNullException(nameof(paramMetadata));
#endif
            var parameterTypeInfo = _reflection.GetType(paramInfo.ParameterType);
            IType genericTypeInfo = null;
            Type[] genericTypeArguments = null;
            if (_reflection.GetType(paramInfo.ParameterType).IsConstructedGenericType)
            {
                var genericTypeDefinition = parameterTypeInfo.GenericTypeDefinition;
                if (genericTypeDefinition != null)
                {
                    genericTypeInfo = _reflection.GetType(genericTypeDefinition);
                    genericTypeArguments = _reflection.GetType(paramInfo.ParameterType).GenericTypeArguments;
                }
            }

            if (paramMetadata.IsDependency)
            {
                return (
                    from contractKey in paramMetadata.ContractKeys ?? Enumerable.Empty<IContractKey>()
                    let contractTypeInfo = _reflection.GetType(contractKey.ContractType)
                    where parameterTypeInfo.IsAssignableFrom(contractTypeInfo) || genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(contractTypeInfo) && genericTypeArguments != null && contractKey.GenericTypeArguments.SequenceEqual(genericTypeArguments)
                    select contractKey).Any();
            }

            if (paramMetadata.StateKey == null)
            {
                return true;
            }

            var stateTypeInfo = _reflection.GetType(paramMetadata.StateKey.StateType);
            return parameterTypeInfo.IsAssignableFrom(stateTypeInfo) || genericTypeInfo != null && genericTypeInfo.IsAssignableFrom(stateTypeInfo);
        }
    }
}