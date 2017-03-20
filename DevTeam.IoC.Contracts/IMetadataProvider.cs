namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]

    public interface IMetadataProvider
    {
        bool TryResolveImplementationType([NotNull] IReflection reflection, [NotNull] Type implementationType, out Type resolvedType, [CanBeNull] ICreationContext creationContext = null);
        
        bool TrySelectConstructor([NotNull] IReflection reflection, [NotNull] Type implementationType, out ConstructorInfo constructor, out Exception error);

        IEnumerable<MethodInfo> GetMethods([NotNull] IReflection reflection, [NotNull] Type implementationType);

        [NotNull]
        IParameterMetadata[] GetParameters([NotNull] IReflection reflection, [NotNull] MethodBase method, ref int stateIndex);
    }
}