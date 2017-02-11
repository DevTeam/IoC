namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class KeyFactory: IKeyFactory
    {
        public ICompositeKey CreateCompositeKey(IEnumerable<IContractKey> contractKey, IEnumerable<ITagKey> tagKeys = null, IEnumerable<IStateKey> stateKeys = null)
        {
            if (contractKey == null) throw new ArgumentNullException(nameof(contractKey));
            return new CompositeKey(contractKey, tagKeys, stateKeys);
        }

        public IContractKey CreateContractKey(Type contractType, bool toResolve)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
            return new ContractKey(contractType, toResolve);
        }

        public IStateKey CreateStateKey(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return new StateKey(index, stateType);
        }

        public ITagKey CreateTagKey(object tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            return new TagKey(tag);
        }
    }
}
