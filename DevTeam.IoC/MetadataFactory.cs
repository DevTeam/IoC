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

        public object Create(ICreationContext creationContext)
        {
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            return _instanceFactory.Create(_parameters.Select(param => ResolveParameter(creationContext, param)).ToArray());
        }

        [CanBeNull]
        private static object ResolveParameter(ICreationContext creationContext, IParameterMetadata parameterMetadata)
        {
            if (parameterMetadata.IsDependency)
            {
                return creationContext.ResolverContext.RegistryContext.Container.Resolve<IResolver>().Key(parameterMetadata.Keys).Instance(parameterMetadata.State ?? EmptyState);
            }

            if(parameterMetadata.Value != null)
            {
                return parameterMetadata.Value;
            }

            return creationContext.StateProvider.GetState(creationContext, parameterMetadata.StateKey);
        }
    }
}