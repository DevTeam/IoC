namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IKeyFactory
    {
        [NotNull]
        ICompositeKey CreateCompositeKey([NotNull] IEnumerable<IContractKey> contractKey, [CanBeNull] IEnumerable<ITagKey> tagKeys = null, [CanBeNull] IEnumerable<IStateKey> stateKeys = null);

        [NotNull]
        IContractKey CreateContractKey([NotNull] Type contractType, bool toResolve);

        [NotNull]
        IStateKey CreateStateKey(int index, [NotNull] Type stateType, bool toResolve);

        [NotNull]
        ITagKey CreateTagKey([NotNull] object tag);
    }
}
