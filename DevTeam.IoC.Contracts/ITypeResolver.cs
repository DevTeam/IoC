namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface ITypeResolver
    {
        void AddReference(string reference);

        void AddUsing(string usingName);

        bool TryResolveType(string typeName, out Type type);
    }
}