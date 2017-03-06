namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class KeyFactory: IKeyFactory
    {
        public ICompositeKey CreateCompositeKey(IEnumerable<IContractKey> contractKey, IEnumerable<ITagKey> tagKeys = null, IEnumerable<IStateKey> stateKeys = null)
        {
#if DEBUG
            if (contractKey == null) throw new ArgumentNullException(nameof(contractKey));
#endif
            return new CompositeKey(contractKey, tagKeys, stateKeys);
        }

        public IContractKey CreateContractKey(Type contractType, bool toResolve)
        {
#if DEBUG
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));
#endif
            return new ContractKey(contractType, toResolve);
        }

        public IStateKey CreateStateKey(int index, Type stateType)
        {
#if DEBUG
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
#endif
            return new StateKey(index, stateType);
        }

        public ITagKey CreateTagKey(object tag)
        {
#if DEBUG
            if (tag == null) throw new ArgumentNullException(nameof(tag));
#endif
            return new TagKey(tag);
        }
    }
}
