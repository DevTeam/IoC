#if NET35 || NET40
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class TypeImpl : IType
    {
        private readonly Type _type;

        public TypeImpl([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            _type = type;
        }

        public Assembly Assembly => _type.Assembly;

        public Type BaseType => _type.BaseType;

        public IEnumerable<ConstructorInfo> Constructors => _type.GetConstructors();

        public bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;

        public Type GenericTypeDefinition => _type.GetGenericTypeDefinition();

        public IEnumerable<PropertyInfo> Properties => _type.GetProperties();

        public IEnumerable<MethodInfo> Methods => _type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty);

        public bool IsEnum => _type.IsEnum;

        public Type Type => _type;

        public bool IsConstructedGenericType => _type.IsGenericType && !_type.IsGenericTypeDefinition;

        public Type[] GenericTypeArguments => _type.GetGenericArguments();

        public Type[] GenericTypeParameters => _type.GetGenericArguments();

        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return _type.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public bool IsAssignableFrom(IType type)
        {
            return _type.IsAssignableFrom(type.Type);
        }

        public MethodInfo GetMethod(string methodName, params Type[] argumenTypes)
        {
            return _type.GetMethod(methodName, argumenTypes);
        }
    }
}
#endif