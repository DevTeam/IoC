namespace DevTeam.IoC
{
    using System;
    using System.Linq;

    using Contracts;

    internal struct ContractKey: IContractKey
    {
        private static readonly Type[] EmptyGenericTypeArguments = new Type[0];
        private readonly int _hashCode;

        public ContractKey(IReflection reflection, Type contractType, bool toResolve)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            ToResolve = toResolve;
            if (reflection.GetIsConstructedGenericType(contractType))
            {
                ContractType = contractType.GetGenericTypeDefinition();
                GenericTypeArguments = reflection.GetGenericTypeArguments(contractType);
            }
            else
            {
                ContractType = contractType;
                GenericTypeArguments = EmptyGenericTypeArguments;
            }

            if (ContractType == null) throw new InvalidOperationException(nameof(ContractType));
            _hashCode = ContractType.GetHashCode();
        }

        public Type ContractType { get; }

        public Type[] GenericTypeArguments { get; }

        public bool ToResolve { get; }

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
            if (ContractType != other.ContractType)
            {
                return false;
            }

            var genericTypeArguments = GenericTypeArguments;
            var otherGenericTypeArguments = other.GenericTypeArguments;
            if (genericTypeArguments.Length == otherGenericTypeArguments.Length)
            {
                if(genericTypeArguments.Length == 0)
                {
                    return true;
                }

                return genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
            }

            if (ToResolve == other.ToResolve)
            {
                return genericTypeArguments.SequenceEqual(otherGenericTypeArguments);
            }

            if ((!ToResolve && genericTypeArguments.Length == 0) || (!other.ToResolve && otherGenericTypeArguments.Length == 0))
            {
                return true;
            }

            return GenericTypeArguments.SequenceEqual(otherGenericTypeArguments);
        }

        public override string ToString()
        {
            return $"{nameof(ContractKey)} [ContractType: {ContractType.Name}, GenericTypeArguments: {string.Join(", ", GenericTypeArguments.Select(i => i.Name).ToArray())}]";
        }
    }
}
