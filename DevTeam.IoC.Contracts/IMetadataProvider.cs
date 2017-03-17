namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    [PublicAPI]

    public interface IMetadataProvider
    {
        bool TryResolveImplementationType([NotNull] IReflection reflection, [NotNull] Type implementationType, out Type resolvedType, [CanBeNull] ICreationContext creationContext = null);
        
        bool TrySelectConstructor([NotNull] IReflection reflection, [NotNull] Type implementationType, out ConstructorInfo constructor, out Exception error);

        [NotNull]
        IParameterMetadata[] GetConstructorParameters([NotNull] IReflection reflection, [NotNull] ConstructorInfo constructor);
    }
}