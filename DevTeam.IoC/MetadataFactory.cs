namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class MetadataFactory: IResolverFactory
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly IParameterMetadata[] _parameters;

        public MetadataFactory(Type implementationType, IInstanceFactoryProvider instanceFactoryProvider, IMetadataProvider metadataProvider = null)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (instanceFactoryProvider == null) throw new ArgumentNullException(nameof(instanceFactoryProvider));
            metadataProvider = metadataProvider ?? AutowiringMetadataProvider.Shared;

            Exception error;
            ConstructorInfo constructor;
            if (!metadataProvider.TrySelectConstructor(implementationType, out constructor, out error))
            {
                throw error;
            }

            _parameters = metadataProvider.GetConstructorParameters(constructor);
            _instanceFactory = instanceFactoryProvider.GetFactory(constructor);
        }

        public object Create(IResolverContext resolverContext)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            return _instanceFactory.Create(_parameters.Select(param => ResolveParameter(resolverContext, param)).ToArray());
        }

        private static object ResolveParameter(IResolverContext resolverContext, IParameterMetadata parameterMetadata)
        {
            if (parameterMetadata.IsDependency)
            {
                return resolverContext.RegistryContext.Container.Resolve().Key(parameterMetadata.Keys).Instance(parameterMetadata.State);
            }

            return resolverContext.StateProvider.GetState(resolverContext, parameterMetadata.StateKey);
        }
    }
}