namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface ITypeResolver
    {
        void AddReference([NotNull] string reference);

        void AddUsingStatement([NotNull] string usingName);

        bool TryResolveType([CanBeNull] string typeName, out Type type);
    }
}