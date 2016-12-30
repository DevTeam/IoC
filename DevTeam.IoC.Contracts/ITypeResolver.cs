namespace DevTeam.IoC.Contracts
{
    using System;

    public interface ITypeResolver
    {
        void AddReference(string reference);

        void AddUsing(string usingName);

        bool TryResolveType(string typeName, out Type type);
    }
}