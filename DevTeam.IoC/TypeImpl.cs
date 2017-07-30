#if !NET35 && !NET40
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal sealed class TypeImpl : IType
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


        public Assembly Assembly => TypeInfo.Assembly;

        public Type BaseType => TypeInfo.BaseType;

        public IEnumerable<ConstructorInfo> Constructors => TypeInfo.DeclaredConstructors;

        public bool IsGenericTypeDefinition => TypeInfo.IsGenericTypeDefinition;

        public Type GenericTypeDefinition => TypeInfo.GetGenericTypeDefinition();

        public IEnumerable<PropertyInfo> Properties => TypeInfo.DeclaredProperties;

        public IEnumerable<MethodInfo> Methods => TypeInfo.DeclaredMethods.Where(i => !i.IsStatic);

        public bool IsEnum => TypeInfo.IsEnum;

        private TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = _type.GetTypeInfo());

        public Type Type
        {
            get
            {
                if (_type == null && _typeInfo != null)
                {
                    _type = _typeInfo.AsType();
                }

                return _type;
            }
        }

        public bool IsConstructedGenericType => Type.IsConstructedGenericType;

        public Type[] GenericTypeArguments => Type.GenericTypeArguments;

        public Type[] GenericTypeParameters => TypeInfo.GenericTypeParameters;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return TypeInfo.GetCustomAttributes<T>();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool IsAssignableFrom(IType type)
        {
            return TypeInfo.IsAssignableFrom(((TypeImpl) type).TypeInfo);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public MethodInfo GetMethod(string methodName, params Type[] argumenTypes)
        {
            return Type.GetRuntimeMethod(methodName, argumenTypes);
        }
    }
}
#endif