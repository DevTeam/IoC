namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class MetadataFactory: IResolverFactory
    {
        private static readonly object[] EmptyState = new object[0];
        private readonly IInstanceFactory _instanceFactory;
        private readonly IParameterMetadata[] _parameters;

        public MetadataFactory(
            [NotNull] Type implementationType,
            [NotNull] IInstanceFactoryProvider instanceFactoryProvider,
            [NotNull] IMetadataProvider metadataProvider)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (instanceFactoryProvider == null) throw new ArgumentNullException(nameof(instanceFactoryProvider));
            if (metadataProvider == null) throw new ArgumentNullException(nameof(metadataProvider));

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

        [CanBeNull]
        private static object ResolveParameter(IResolverContext resolverContext, IParameterMetadata parameterMetadata)
        {
            if (parameterMetadata.IsDependency)
            {
                return resolverContext.RegistryContext.Container.Resolve<IResolver>().Key(parameterMetadata.Keys).Instance(parameterMetadata.State ?? EmptyState);
            }

            if(parameterMetadata.Value != null)
            {
                return parameterMetadata.Value;
            }

            return resolverContext.StateProvider.GetState(resolverContext, parameterMetadata.StateKey);
        }
    }
}