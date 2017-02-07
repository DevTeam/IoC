namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IKeyFactory
    {
        ICompositeKey CreateCompositeKey(IContractKey[] contractKey, ITagKey[] tagKeys, IStateKey[] stateKeys);

        IContractKey CreateContractKey(Type contractType, bool toResolve);

        IStateKey CreateStateKey(int index, Type stateType);

        ITagKey CreateTagKey(object tag);
    }
}
