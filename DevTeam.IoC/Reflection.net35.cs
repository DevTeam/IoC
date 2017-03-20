#if NET35 || NET40
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class Reflection : IReflection
    {
        public IEnumerable<IType> GetDefinedTypes(Assembly assembly)
        {
            return assembly.GetTypes().Select(type => (IType)new TypeImpl(type));
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameterInfo)
            where T : Attribute
        {
            return parameterInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public IType GetType(Type type)
        {
            return new TypeImpl(type);
        }
    }
}
#endif