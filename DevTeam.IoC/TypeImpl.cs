#if !NET35 && !NET40
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class TypeImpl : IType
    {
        [CanBeNull]
        private TypeInfo _typeInfo;

        [CanBeNull]
        private Type _type;

        public TypeImpl([NotNull] TypeInfo typeInfo)
        {
#if DEBUG
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));
#endif
            _typeInfo = typeInfo;
        }

        public TypeImpl([NotNull] Type type)
        {
#if DEBUG
            if (type == null) throw new ArgumentNullException(nameof(type));
#endif
            _type = type;
        }

        public IEnumerable<ConstructorInfo> Constructors => TypeInfo.DeclaredConstructors;

        public bool IsGenericTypeDefinition => TypeInfo.IsGenericTypeDefinition;

        public Type GenericTypeDefinition => TypeInfo.GetGenericTypeDefinition();

        public IEnumerable<PropertyInfo> Properties => TypeInfo.DeclaredProperties;

        public bool IsEnum => TypeInfo.IsEnum;

        private TypeInfo TypeInfo
        {
            get
            {
                if (_typeInfo == null)
                {
                    _typeInfo = _type.GetTypeInfo();
                }

                return _typeInfo;
            }
        }

        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    _type = _typeInfo.AsType();
                }

                return _type;
            }
        }

        public bool IsConstructedGenericType => Type.IsConstructedGenericType;

        public Type[] GenericTypeArguments => Type.GenericTypeArguments;

        public IEnumerable<MethodInfo> Methods => TypeInfo.DeclaredMethods;

        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return TypeInfo.GetCustomAttributes<T>();
        }

        public bool IsAssignableFrom(IType type)
        {
            return TypeInfo.IsAssignableFrom(((TypeImpl) type).TypeInfo);
        }

        public MethodInfo GetMethod(string methodName, params Type[] argumenTypes)
        {
            return Type.GetRuntimeMethod(methodName, argumenTypes);
        }
    }
}
#endif