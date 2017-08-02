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
            var compositeKey = obj as ICompositeKey;
            if (compositeKey != null)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                return compositeKey.Equals(this);
            }

            var contractKey = obj as IContractKey;
            return contractKey != null && Equals(contractKey);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(IContractKey other)
        {
            if (_contractType != other.ContractType)
            {
                return false;
            }

            var otherGenericTypeArguments = other.GenericTypeArguments;
            if (_genericTypeArguments.Length == otherGenericTypeArguments.Length)
            {
                if(_genericTypeArguments.Length == 0)
                {
                    return true;
                }

#if !NET35
                return ((System.Collections.IStructuralEquatable)_genericTypeArguments).Equals(otherGenericTypeArguments, System.Collections.StructuralComparisons.StructuralEqualityComparer);
#else
                return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
#endif
            }

            if (_resolving == other.Resolving)
            {
#if !NET35
                return ((System.Collections.IStructuralEquatable)_genericTypeArguments).Equals(otherGenericTypeArguments, System.Collections.StructuralComparisons.StructuralEqualityComparer);
#else
                return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
#endif
            }

            if ((!_resolving && _genericTypeArguments.Length == 0) || (!other.Resolving && otherGenericTypeArguments.Length == 0))
            {
                return true;
            }

#if !NET35
            return ((System.Collections.IStructuralEquatable)_genericTypeArguments).Equals(otherGenericTypeArguments, System.Collections.StructuralComparisons.StructuralEqualityComparer);
#else
            return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
#endif
        }

        public override string ToString()
        {
            return $"{nameof(ContractKey)} [ContractType: {_contractType.Name}, GenericTypeArguments: {string.Join(", ", _genericTypeArguments.Select(i => i.Name).ToArray())}, Resolving: {_resolving}]";
        }
    }
}
