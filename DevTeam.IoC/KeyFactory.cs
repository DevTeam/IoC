namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class KeyFactory: IKeyFactory
    {
        public ICompositeKey CreateCompositeKey(IContractKey[] contractKey, ITagKey[] tagKeys, IStateKey[] stateKeys)
        {
            if (contractKey == null) throw new ArgumentNullException(nameof(contractKey));
            if (tagKeys == null) throw new ArgumentNullException(nameof(tagKeys));
            if (stateKeys == null) throw new ArgumentNullException(nameof(stateKeys));
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
