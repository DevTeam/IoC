namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

#if !NET35 && !NET40
    internal class Reflection : IReflection
    {
        public IEnumerable<ITypeInfo> GetDefinedTypes(Assembly assembly)
        {
            return assembly.DefinedTypes.Select(type => (ITypeInfo)new TypeInfo(type));
        }

        public bool GetIsConstructedGenericType(Type type)
        {
            return type.IsConstructedGenericType;
        }

        public Type[] GetGenericTypeArguments(Type type)
        {
            return type.GenericTypeArguments;
        }

        public MethodInfo GetRuntimeMethod(Type type, string methodName, params Type[] argumenTypes)
        {
            return type.GetRuntimeMethod(methodName, argumenTypes);
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : Attribute
        {
            return memberInfo.GetCustomAttributes<T>(inherit);
        }

        public T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            return propertyInfo.GetCustomAttribute<T>();
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameterInfo)
            where T : Attribute
        {
            return parameterInfo.GetCustomAttributes<T>();
        }

        public T GetCustomAttribute<T>(ConstructorInfo constructorInfo)
            where T : Attribute
        {
            return constructorInfo.GetCustomAttribute<T>();
        }

        public ITypeInfo GetTypeInfo(Type type)
        {
            return new TypeInfo(type.GetTypeInfo());
        }
    }
#endif
}