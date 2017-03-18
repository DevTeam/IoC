namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface ITypeInfo
    {
        [NotNull]
        IEnumerable<ConstructorInfo> DeclaredConstructors { get; }

        [NotNull]
        Type[] GenericTypeParameters { get; }

        bool IsEnum { get; }

        bool IsGenericTypeDefinition { get; }

        IEnumerable<PropertyInfo> DeclaredProperties { get; }

        [NotNull]
        Type AsType();

        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>() where T : Attribute;

        [CanBeNull]
        Type GetGenericTypeDefinition();

        bool IsAssignableFrom([NotNull] ITypeInfo stateTypeInfo);
    }
}