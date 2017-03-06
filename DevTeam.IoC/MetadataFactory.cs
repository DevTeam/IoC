namespace DevTeam.IoC
{
    using System;
    using System.Reflection;
    using Contracts;

    internal class MetadataFactory: IResolverFactory
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly IParameterMetadata[] _parameters;
        private object[] _parametersArray;
        private readonly IKey[] _keys;

        public MetadataFactory(
            [NotNull] Type implementationType,
            [NotNull] IInstanceFactoryProvider instanceFactoryProvider,
            [NotNull] IMetadataProvider metadataProvider,
            [NotNull] IKeyFactory keyFactory)
        {
#if DEBUG
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (instanceFactoryProvider == null) throw new ArgumentNullException(nameof(instanceFactoryProvider));
            if (metadataProvider == null) throw new ArgumentNullException(nameof(metadataProvider));
#endif
            Exception error;
            ConstructorInfo constructor;
            if (!metadataProvider.TrySelectConstructor(implementationType, out constructor, out error))
            {
                throw error;
            }

            _parameters = metadataProvider.GetConstructorParameters(constructor);
            _instanceFactory = instanceFactoryProvider.GetFactory(constructor);
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
        }

        public object Create(ICreationContext creationContext)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            for (var i = 0; i < _parametersArray.Length; i++)
            {
                _parametersArray[i] = ResolveParameter(i, creationContext);
            }

            return _instanceFactory.Create(_parametersArray);
        }

        [CanBeNull]
        private object ResolveParameter(
            int index,
            [NotNull] ICreationContext creationContext)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            IParameterMetadata parameterMetadata = _parameters[index];
            if (parameterMetadata.IsDependency)
            {
                var key = _keys[index];
                var container = creationContext.ResolverContext.RegistryContext.Container;
                IResolverContext ctx;
                if (!container.TryCreateResolverContext(key, out ctx))
                {
                    throw new InvalidOperationException(GetCantResolveErrorMessage(container, key));
                }
                return container.Resolve(ctx, ParamsStateProvider.Create(parameterMetadata.State));
            }

            if(parameterMetadata.Value != null)
            {
                return parameterMetadata.Value;
            }

            return creationContext.StateProvider.GetState(creationContext, parameterMetadata.StateKey);
        }

        private string GetCantResolveErrorMessage(IResolver resolver, IKey key)
        {
            return $"Can't resolve {key}.{Environment.NewLine}{Environment.NewLine}{resolver}";
        }
    }
}