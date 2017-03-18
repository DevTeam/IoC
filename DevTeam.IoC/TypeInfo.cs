namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

#if !NET35 && !NET40
    internal class TypeInfo : ITypeInfo
    {
        private readonly System.Reflection.TypeInfo _typeInfo;

        public TypeInfo([NotNull] System.Reflection.TypeInfo typeInfo)
        {
#if DEBUG
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));
#endif
            _typeInfo = typeInfo;
        }

        public IEnumerable<ConstructorInfo> DeclaredConstructors => _typeInfo.DeclaredConstructors;

        public bool IsGenericTypeDefinition => _typeInfo.IsGenericTypeDefinition;

        public IEnumerable<PropertyInfo> DeclaredProperties => _typeInfo.DeclaredProperties;

        public Type[] GenericTypeParameters => _typeInfo.GenericTypeParameters;

        public bool IsEnum => _typeInfo.IsEnum;

        public Type AsType()
        {
            return _typeInfo.AsType();
        }

        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return _typeInfo.GetCustomAttributes<T>();
        }

        public bool IsAssignableFrom(ITypeInfo stateTypeInfo)
        {
            return _typeInfo.IsAssignableFrom(((TypeInfo) stateTypeInfo)._typeInfo);
        }

        public Type GetGenericTypeDefinition()
        {
            return _typeInfo.GetGenericTypeDefinition();
        }
    }
#endif
}