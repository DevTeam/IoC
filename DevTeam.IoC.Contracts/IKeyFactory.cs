namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IKeyFactory
    {
        [NotNull]
        ICompositeKey CreateCompositeKey([NotNull] IContractKey[] contractKey, [NotNull] ITagKey[] tagKeys, [NotNull] IStateKey[] stateKeys);

        [NotNull]
        IContractKey CreateContractKey([NotNull] Type contractType, bool toResolve);

        [NotNull]
        IStateKey CreateStateKey(int index, [NotNull] Type stateType);

        [NotNull]
        ITagKey CreateTagKey([NotNull] object tag);
    }
}
