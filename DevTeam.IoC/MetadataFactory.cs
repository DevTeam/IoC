namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal sealed class MetadataFactory: IInstanceFactory
    {
        private readonly Constructor _constructor;
        private readonly IParameterMetadata[] _parameters;
        private object[] _parametersArray;
        private readonly IKey[] _keys;
        [CanBeNull] private List<MethodData> _methods;

        public MetadataFactory(
            [NotNull] Type implementationType,
            [NotNull] IMethodFactory methodFactory,
            [NotNull] IMetadataProvider metadataProvider,
            [NotNull] IKeyFactory keyFactory)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (methodFactory == null) throw new ArgumentNullException(nameof(methodFactory));
            if (metadataProvider == null) throw new ArgumentNullException(nameof(metadataProvider));
#endif
            if (!metadataProvider.TrySelectConstructor(implementationType, out ConstructorInfo constructor, out Exception error))
            {
                throw error;
            }

            var stateIndex = 0;
            _parameters = metadataProvider.GetParameters(constructor, ref stateIndex);
            _constructor = methodFactory.CreateConstructor(constructor);
            var len = _parameters.Length;
            _parametersArray = new object[len];
            _keys = new IKey[len];
            for (var i = 0; i < len; i++)
            {
                var parameter = _parameters[i];
                if (!parameter.IsDependency)
                {
                    continue;
                }

                _keys[i] = keyFactory.CreateCompositeKey(parameter.ContractKeys, parameter.TagKeys, parameter.StateKeys);
            }

            foreach (var initMethod in metadataProvider.GetMethods(implementationType))
            {
                var methodData = new MethodData();
                methodData.Parameters = metadataProvider.GetParameters(initMethod, ref stateIndex);
                len = methodData.Parameters.Length;
                methodData.ParametersArray = new object[len];
                methodData.Keys = new IKey[len];
                methodData.Method = methodFactory.CreateMethod(implementationType, initMethod);
                for (var i = 0; i < len; i++)
                {
                    var parameter = methodData.Parameters[i];
                    if (!parameter.IsDependency)
                    {
                        continue;
                    }

                    methodData.Keys[i] = keyFactory.CreateCompositeKey(parameter.ContractKeys, parameter.TagKeys, parameter.StateKeys);
                }

                if (_methods == null)
                {
                    _methods = new List<MethodData>();
                }

                _methods.Add(methodData);
            }
        }

        public object Create(ICreationContext creationContext)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            for (var i = 0; i < _parametersArray.Length; i++)
            {
                _parametersArray[i] = ResolveParameter(i, creationContext, _parameters, _keys);
            }

            try
            {
                object instance;
                try
                {
                    instance = _constructor(_parametersArray);
                }
                catch (Exception ex)
                {
                    throw new ContainerException($"Error during creation of object using constructor {_constructor}.\nDetails:\n{creationContext}", ex);
                }

                if (_methods == null)
                {
                    return instance;
                }

                foreach (var method in _methods)
                {
                    for (var i = 0; i < method.ParametersArray.Length; i++)
                    {
                        method.ParametersArray[i] = ResolveParameter(i, creationContext, method.Parameters, method.Keys);
                    }

                    try
                    {
                        method.Method(instance, method.ParametersArray);
                    }
                    catch (Exception exception)
                    {
                        throw new ContainerException($"Error during an invokation of method {method}.\nDetails:\n{creationContext}", exception);
                    }
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw new ContainerException($"Error during creation of object.\nDetails:\n{creationContext}", ex);
            }
        }

        [CanBeNull]
        private static object ResolveParameter(int index, [NotNull] ICreationContext creationContext, [NotNull] IParameterMetadata[] parameters, [NotNull] IKey[] keys)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (index < 0 || index >= keys.Length) throw new ArgumentOutOfRangeException(nameof(index));
#endif
            var parameterMetadata = parameters[index];
            if (parameterMetadata.IsDependency)
            {
                var key = keys[index];
                var container = creationContext.ResolverContext.RegistryContext.Container;
                if (!container.TryCreateResolverContext(key, out IResolverContext ctx))
                {
                    throw new ContainerException(GetCantResolveErrorMessage(container, key));
                }

                return container.Resolve(ctx, ParamsStateProvider.Create(parameterMetadata.State));
            }

            if (parameterMetadata.Value != null)
            {
                return parameterMetadata.Value;
            }

            return creationContext.StateProvider.GetState(creationContext, parameterMetadata.StateKey);
        }

        private static string GetCantResolveErrorMessage(IResolver resolver, IKey key)
        {
            return $"Can't resolve {key}.\nDetails:\n{resolver}";
        }

        private struct MethodData
        {
            public IParameterMetadata[] Parameters;

            public object[] ParametersArray;

            public IKey[] Keys;

            public Method Method;
        }
    }
}