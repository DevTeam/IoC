namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IReflection
    {
        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>([NotNull] ParameterInfo parameterInfo) where T : Attribute;

        [NotNull]
        IEnumerable<T> GetCustomAttributes<T>([NotNull] MemberInfo memberInfo, bool inherit) where T : Attribute;

        [NotNull]
        IEnumerable<IType> GetDefinedTypes([NotNull] Assembly assembly);

        [NotNull]
        IType GetType([NotNull] Type type);
    }
}