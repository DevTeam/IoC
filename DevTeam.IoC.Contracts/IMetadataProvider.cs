namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    [PublicAPI]

    public interface IMetadataProvider
    {
        [NotNull]
        Type ResolveImplementationType([NotNull] IResolverContext resolverContext, [NotNull] Type implementationType);

        bool TrySelectConstructor([NotNull] Type implementationType, out ConstructorInfo constructor, out Exception error);

        [NotNull]
        IParameterMetadata[] GetConstructorParameters([NotNull] ConstructorInfo constructor);
    }
}