namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IType
    {
        Type BaseType { get; }

        IEnumerable<ConstructorInfo> Constructors { [NotNull] get; }

        IEnumerable<PropertyInfo> Properties { [NotNull] get; }

        IEnumerable<MethodInfo> Methods { [NotNull] get; }

        bool IsEnum { get; }

        bool IsGenericTypeDefinition { get; }

        Type GenericTypeDefinition { [CanBeNull] get; }

        Type[] GenericTypeArguments { [NotNull] get; }

        Type[] GenericTypeParameters { [NotNull] get; }

        bool IsConstructedGenericType { get; }

        Type Type { [NotNull] get; }

        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>() where T : Attribute;

        bool IsAssignableFrom(IType type);

        [CanBeNull]
        MethodInfo GetMethod([NotNull] string methodName, [NotNull] params Type[] argumenTypes);
    }
}