namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

#if NET35 || NET40
    internal class TypeInfo : ITypeInfo
    {
        private readonly Type _type;

        public TypeInfo([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            _type = type;
        }

        public IEnumerable<ConstructorInfo> DeclaredConstructors => _type.GetConstructors();

        public bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;

        public IEnumerable<PropertyInfo> DeclaredProperties => _type.GetProperties();

        public Type[] GenericTypeParameters => _type.GetGenericArguments();

        public bool IsEnum => _type.IsEnum;

        public Type AsType()
        {
            return _type;
        }

        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return _type.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public bool IsAssignableFrom(ITypeInfo stateTypeInfo)
        {
            return _type.IsAssignableFrom(stateTypeInfo.AsType());
        }

        public Type GetGenericTypeDefinition()
        {
            return _type.GetGenericTypeDefinition();
        }
    }
#endif
}