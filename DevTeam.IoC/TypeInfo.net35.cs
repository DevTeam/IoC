namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
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

        public IEnumerable<ConstructorInfo> DeclaredConstructors { get; }

        public bool IsGenericTypeDefinition { get; }

        public IEnumerable<PropertyInfo> DeclaredProperties { get; }

        public Type[] GenericTypeParameters { get; }

        public bool IsEnum { get; }

        public Type AsType()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            throw new NotImplementedException();
        }

        public bool IsAssignableFrom(ITypeInfo stateTypeInfo)
        {
            throw new NotImplementedException();
        }

        public Type GetGenericTypeDefinition()
        {
            throw new NotImplementedException();
        }
    }
#endif
}