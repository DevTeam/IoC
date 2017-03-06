namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    [PublicAPI]

    public interface IMetadataProvider
    {
        bool TryResolveImplementationType([NotNull] Type implementationType, out Type resolvedType, [CanBeNull] ICreationContext creationContext = null);
        
        bool TrySelectConstructor([NotNull] Type implementationType, out ConstructorInfo constructor, out Exception error);

        [NotNull]
        IParameterMetadata[] GetConstructorParameters([NotNull] ConstructorInfo constructor);
    }
}