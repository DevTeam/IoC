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
        [CanBeNull] private readonly IParameterMetadata[] _constructorParams;
        [CanBeNull] private MethodMetadata[] _methods;
        [CanBeNull] private PropertyMetadata[] _properties;
        [CanBeNull] private IDictionary<MethodInfo, IParameterMetadata[]> _methodsDict;

        public ManualMetadataProvider(
            [NotNull] IMetadataProvider defaultMetadataProvider,
            [NotNull] TypeMetadata typeMetadata)
        {
#if DEBUG
            if (defaultMetadataProvider == null) throw new ArgumentNullException(nameof(defaultMetadataProvider));
            if (typeMetadata == null) throw new ArgumentNullException(nameof(typeMetadata));
#endif
            _defaultMetadataProvider = defaultMetadataProvider;
            _constructorParams = typeMetadata.Constructor?.Parameters.ToArray();
            _methods = typeMetadata.Methods?.ToArray();
            _properties = typeMetadata.Properties?.ToArray();
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
            if (_constructorParams == null)
            {
                return _defaultMetadataProvider.TrySelectConstructor(reflection, implementationType, out constructor, out error);
            }

            var typeInfo = reflection.GetType(implementationType);
            constructor = typeInfo.Constructors.FirstOrDefault(ctor => MatchMethod(reflection, ctor));
            if (constructor == null)
            {
                return _defaultMetadataProvider.TrySelectConstructor(reflection, implementationType, out constructor, out error);
            }

            error = default(Exception);
            return true;
        }

        public IEnumerable<MethodInfo> GetMethods(IReflection reflection, Type implementationType)
        {
            return GetMethodsInternal(reflection, implementationType).Keys;
        }

        private IDictionary<MethodInfo, IParameterMetadata[]> GetMethodsInternal(IReflection reflection, Type implementationType)
        {
            if (_methodsDict != null)
            {
                return _methodsDict;
            }

            _methodsDict = new Dictionary<MethodInfo, IParameterMetadata[]>();
            var type = reflection.GetType(implementationType);
            var methods = type.Methods.ToArray();

            if (_methods != null)
            {
                var methodList =
                    from methodMetadata in _methods
                    join method in methods on methodMetadata.Name equals method.Name
                    where
                        method.GetParameters().Zip(methodMetadata.Parameters, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                        .All(i => MatchParameter(reflection, i.ctorParam, i.bindingParam))
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
                    where MatchParameter(reflection, pars[0], propertyMetadata.Parameter)
                    select new { method, propertyMetadata.Parameter };

                foreach (var item in methodList)
                {
                    _methodsDict.Add(item.method, new[] {item.Parameter});
                }
            }

            return _methodsDict;
        }

        public IParameterMetadata[] GetParameters(IReflection reflection, MethodBase method, ref int stateIndex)
        {
#if DEBUG
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
#endif
            var methodInfo = method as MethodInfo;
            if (methodInfo != null)
            {
                var methodsDict = GetMethodsInternal(reflection, method.DeclaringType);
                if (methodsDict.TryGetValue(methodInfo, out IParameterMetadata[] pars))
                {
                    return pars;
                }
            }

            return _constructorParams ?? _defaultMetadataProvider.GetParameters(reflection, method, ref stateIndex);
        }

        private bool MatchMethod([NotNull] IReflection reflection, [NotNull] MethodBase method)
        {
#if DEBUG
            if (reflection == null) throw new ArgumentNullException(nameof(reflection));
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