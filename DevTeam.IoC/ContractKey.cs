namespace DevTeam.IoC
{
    using System;
    using System.Linq;

    using Contracts;

    internal struct ContractKey: IContractKey
    {
        private static readonly Type[] EmptyGenericTypeArguments = new Type[0];
        private readonly int _hashCode;
        private readonly Type[] _genericTypeArguments;
        private readonly Type _contractType;
        private readonly bool _resolving;

        public ContractKey(IReflection reflection, Type contractType, bool resolving)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            _resolving = resolving;
            var typeInfo = reflection.GetType(contractType);
            if (typeInfo.IsConstructedGenericType)
            {
                _contractType = contractType.GetGenericTypeDefinition();
                if (_contractType == null)
                {
                    throw new ContainerException($"Generic type defenition for type \"{contractType}\" is not defined.");
                }

                _genericTypeArguments = typeInfo.GenericTypeArguments;
            }
            else
            {
                _contractType = contractType;
                _genericTypeArguments = EmptyGenericTypeArguments;
            }

            _hashCode = _contractType.GetHashCode();
        }

        public Type ContractType => _contractType;

        public Type[] GenericTypeArguments => _genericTypeArguments;

        public bool Resolving => _resolving;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            switch (obj)
            {
                case ContractKey contractKey:
                    return Equals(contractKey);

                case IContractKey contractKey:
                    return Equals(contractKey);

                case ICompositeKey compositeKey:
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    return compositeKey.Equals(this);

                default: throw new ContainerException($"Ivalid ${obj} to compare");
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(ContractKey other)
        {
            return _contractType == other._contractType && Equals(other, other._genericTypeArguments);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IContractKey other)
        {
            return _contractType == other.ContractType && Equals(other, other.GenericTypeArguments);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IContractKey other, Type[] otherGenericTypeArguments)
        {
            var genericTypeArgumentsLength = _genericTypeArguments.Length;
            var otherGenericTypeArgumentsLength = otherGenericTypeArguments.Length;
            if (genericTypeArgumentsLength == otherGenericTypeArgumentsLength)
            {
                if (genericTypeArgumentsLength == 0)
                {
                    return true;
                }

                return Arrays.SequenceEqual(_genericTypeArguments, otherGenericTypeArguments);
            }

            if (_resolving == other.Resolving)
            {
                return Arrays.SequenceEqual(_genericTypeArguments, otherGenericTypeArguments);
            }

            if (!_resolving && genericTypeArgumentsLength == 0 || !other.Resolving && otherGenericTypeArgumentsLength == 0)
            {
                return true;
            }

            return Arrays.SequenceEqual(_genericTypeArguments, otherGenericTypeArguments);
        }

        public override string ToString()
        {
            return $"{nameof(ContractKey)} [ContractType: {_contractType.Name}, GenericTypeArguments: {string.Join(", ", _genericTypeArguments.Select(i => i.Name).ToArray())}, Resolving: {_resolving}]";
        }
    }
}
