#if NET35 || NET40
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class Reflection : IReflection
    {
        public static readonly IReflection Shared = new Reflection();

        private Reflection()
        {
        }

        public IEnumerable<IType> GetDefinedTypes(Assembly assembly)
        {
#if DEBUG
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
#endif
            return assembly.GetTypes().Select(type => (IType)new TypeImpl(type));
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : Attribute
        {
#if DEBUG
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
#endif
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameterInfo)
            where T : Attribute
        {
#if DEBUG
            if (parameterInfo == null) throw new ArgumentNullException(nameof(parameterInfo));
#endif
            return parameterInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public IType GetType(Type type)
        {
#if DEBUG
            if (type == null) throw new ArgumentNullException(nameof(type));
#endif
            return new TypeImpl(type);
        }
    }
}
#endif