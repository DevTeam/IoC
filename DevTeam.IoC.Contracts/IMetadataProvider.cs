namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]

    public interface IMetadataProvider
    {
        bool TryResolveType([NotNull] Type implementationType, out Type resolvedType, [CanBeNull] CreationContext? creationContext = null);
        
        bool TrySelectConstructor([NotNull] Type implementationType, out ConstructorInfo constructor, out Exception error);

        IEnumerable<MethodInfo> GetMethods([NotNull] Type implementationType);

        [NotNull]
        IParameterMetadata[] GetParameters([NotNull] MethodBase method, ref int stateIndex);
    }
}