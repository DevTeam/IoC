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
        private readonly IParameterMetadata[] _ctorParams;

        public ManualMetadataProvider(
            [NotNull] IMetadataProvider defaultMetadataProvider,
            [NotNull] IEnumerable<IParameterMetadata> ctorParams)
        {
            if (defaultMetadataProvider == null) throw new ArgumentNullException(nameof(defaultMetadataProvider));
            if (ctorParams == null) throw new ArgumentNullException(nameof(ctorParams));
            _defaultMetadataProvider = defaultMetadataProvider;
            _ctorParams = ctorParams.ToArray();
        }

        public Type ResolveImplementationType(IResolverContext resolverContext, Type implementationType)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            return _defaultMetadataProvider.ResolveImplementationType(resolverContext, implementationType);
        }

        public bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            var typeInfo = implementationType.GetTypeInfo();
            constructor = typeInfo.DeclaredConstructors.Where(MatchConstructor).FirstOrDefault();
            error = default(Exception);
            return constructor != null;
        }

        public IParameterMetadata[] GetConstructorParameters(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
            return _ctorParams;
        }

        private bool MatchConstructor(ConstructorInfo ctor)
        {
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length != _ctorParams.Length)
            {
                return false;
            }

            return ctorParams
                .Zip(_ctorParams, (ctorParam, bindingParam) => new { ctorParam, bindingParam })
                .Any(i => MatchParameter(i.ctorParam, i.bindingParam));
        }

        private static bool MatchParameter(ParameterInfo ctorParam, IParameterMetadata bindingCtorParam)
        {
            return ctorParam.ParameterType == bindingCtorParam.ImplementationType;
        }
    }
}