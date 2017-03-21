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
            if (reflection.GetType(contractType).IsConstructedGenericType)
            {
                _contractType = contractType.GetGenericTypeDefinition();
                _genericTypeArguments = reflection.GetType(contractType).GenericTypeArguments;
            }
            else
            {
                _contractType = contractType;
                _genericTypeArguments = EmptyGenericTypeArguments;
            }

            if (_contractType == null) throw new InvalidOperationException(nameof(ContractType));
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

                return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
            }

            if (_resolving == other.Resolving)
            {
                return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
            }

            if ((!_resolving && _genericTypeArguments.Length == 0) || (!other.Resolving && otherGenericTypeArguments.Length == 0))
            {
                return true;
            }

            return _genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
        }

        public override string ToString()
        {
            return $"{nameof(ContractKey)} [ContractType: {_contractType.Name}, GenericTypeArguments: {string.Join(", ", _genericTypeArguments.Select(i => i.Name).ToArray())}, Resolving: {_resolving}]";
        }
    }
}
