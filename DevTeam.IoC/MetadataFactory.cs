namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class MetadataFactory: IResolverFactory
    {
        private static readonly object[] EmptyState = new object[0];
        private readonly IInstanceFactory _instanceFactory;
        private readonly IParameterMetadata[] _parameters;
        private object[] _parametersArray;

        public MetadataFactory(
            [NotNull] Type implementationType,
            [NotNull] IInstanceFactoryProvider instanceFactoryProvider,
            [NotNull] IMetadataProvider metadataProvider)
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
            _parametersArray = new object[_parameters.Length];
        }

        public object Create(ICreationContext creationContext)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            for (var i = 0; i < _parametersArray.Length; i++)
            {
                _parametersArray[i] = ResolveParameter(creationContext, _parameters[i]);
            }

            return _instanceFactory.Create(_parametersArray);
        }

        [CanBeNull]
        private static object ResolveParameter(
            [NotNull] ICreationContext creationContext,
            [NotNull] IParameterMetadata parameterMetadata)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            if (parameterMetadata == null) throw new ArgumentNullException(nameof(parameterMetadata));
#endif
            if (parameterMetadata.IsDependency)
            {
                var container = creationContext.ResolverContext.RegistryContext.Container;
                var resolver = container.Resolve();
                if (parameterMetadata.ContractKeys != null)
                {
                    resolver.Key((IEnumerable<IContractKey>)parameterMetadata.ContractKeys);
                }

                if (parameterMetadata.StateKeys != null)
                {
                    foreach (var stateKey in parameterMetadata.StateKeys)
                    {
                        resolver.Key(stateKey);
                    }
                }

                if (parameterMetadata.TagKeys != null)
                {
                    foreach (var tagKey in parameterMetadata.TagKeys)
                    {
                        resolver.Key(tagKey);
                    }
                }

                return resolver.Instance(parameterMetadata.State ?? EmptyState);
            }

            if(parameterMetadata.Value != null)
            {
                return parameterMetadata.Value;
            }

            return creationContext.StateProvider.GetState(creationContext, parameterMetadata.StateKey);
        }
    }
}