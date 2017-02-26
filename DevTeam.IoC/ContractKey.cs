namespace DevTeam.IoC
{
    using System;
    using System.Linq;

    using Contracts;

    internal struct ContractKey: IContractKey
    {
        private static readonly Type[] EmptyGenericTypeArguments = new Type[0];

        public ContractKey(Type contractType, bool toResolve)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            ToResolve = toResolve;
            if (contractType.IsConstructedGenericType)
            {
                ContractType = contractType.GetGenericTypeDefinition();
                GenericTypeArguments = contractType.GenericTypeArguments;
            }
            else
            {
                ContractType = contractType;
                GenericTypeArguments = EmptyGenericTypeArguments;
            }
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
            return ContractType.GetHashCode();
        }

        private bool Equals(IContractKey other)
        {
            if (ContractType != other.ContractType)
            {
                return false;
            }

            if (GenericTypeArguments.Length == other.GenericTypeArguments.Length)
            {
                if(GenericTypeArguments.Length == 0)
                {
                    return true;
                }

                return GenericTypeArguments.SequenceEqual(other.GenericTypeArguments);
            }

            if (ToResolve == other.ToResolve)
            {
                return GenericTypeArguments.SequenceEqual(other.GenericTypeArguments);
            }

            if ((!ToResolve && GenericTypeArguments.Length == 0) || (!other.ToResolve && other.GenericTypeArguments.Length == 0))
            {
                return true;
            }

            return GenericTypeArguments.SequenceEqual(other.GenericTypeArguments);
        }

        public override string ToString()
        {
            return $"{nameof(ContractKey)} [ContractType: {ContractType.Name}, GenericTypeArguments: {string.Join(", ", GenericTypeArguments.Select(i => i.Name))}]";
        }
    }
}
