namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

#if NET35 || NET40
    internal class Reflection : IReflection
    {
        public IEnumerable<ITypeInfo> GetDefinedTypes(Assembly assembly)
        {
            return assembly.GetTypes().Select(type => (ITypeInfo)new TypeInfo(type));
        }

        public bool GetIsConstructedGenericType(Type type)
        {
            return type.IsGenericType;
        }

        public Type[] GetGenericTypeArguments(Type type)
        {
            return type.GetGenericArguments();
        }

        public MethodInfo GetRuntimeMethod(Type type, string methodName, params Type[] argumenTypes)
        {
            return type.GetMethod(methodName, argumenTypes);
        }

        public IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
            where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            return propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }

        public IEnumerable<T> GetCustomAttributes<T>(ParameterInfo parameterInfo)
            where T : Attribute
        {
            return parameterInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public T GetCustomAttribute<T>(ConstructorInfo constructorInfo)
            where T : Attribute
        {
            return constructorInfo.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }

        public ITypeInfo GetTypeInfo(Type type)
        {
            return new TypeInfo(type);
        }
    }

#endif
}