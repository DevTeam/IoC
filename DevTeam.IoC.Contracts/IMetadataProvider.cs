﻿namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    public interface IMetadataProvider
    {
        Type ResolveImplementationType(IResolverContext resolverContext, Type type);

        bool TrySelectConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error);

        IParameterMetadata[] GetConstructorArguments(ConstructorInfo constructor);
    }
}