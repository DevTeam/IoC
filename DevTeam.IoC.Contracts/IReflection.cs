namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [PublicAPI]
    public interface IReflection
    {
        [CanBeNull]
        T GetCustomAttribute<T>([NotNull] ConstructorInfo constructorInfo) where T : Attribute;

        [CanBeNull]
        T GetCustomAttribute<T>([NotNull] PropertyInfo propertyInfo) where T : Attribute;

        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>([NotNull] ParameterInfo parameterInfo) where T : Attribute;

        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit) where T : Attribute;

        [NotNull]
        IEnumerable<ITypeInfo> GetDefinedTypes([NotNull] Assembly assembly);

        [NotNull]
        Type[] GetGenericTypeArguments([NotNull] Type type);

        bool GetIsConstructedGenericType([NotNull] Type type);

        [CanBeNull]
        MethodInfo GetRuntimeMethod([NotNull] Type type, [NotNull] string methodName, [NotNull] params Type[] argumenTypes);

        [NotNull]
        ITypeInfo GetTypeInfo([NotNull] Type type);
    }
}