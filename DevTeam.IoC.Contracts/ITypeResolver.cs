namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface ITypeResolver
    {
        bool TryResolveType([NotNull] IEnumerable<Assembly> references, [NotNull] IEnumerable<string> usings, [CanBeNull] string typeName, out Type type);
    }
}