namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IToken<out TToken>
    {
        [NotNull]
        TToken Key([NotNull] IEnumerable<IKey> keys);

        [NotNull]
        TToken Key([NotNull] params IKey[] keys);

        [NotNull]
        TToken Key([NotNull] IEnumerable<IContractKey> keys);

        [NotNull]
        TToken Key([NotNull] IStateKey key);

        TToken Key([NotNull] ITagKey key);

        TToken Key([NotNull] ICompositeKey compositeKey);

        [NotNull]
        TToken Contract([NotNull] params Type[] contractTypes);

        [NotNull]
        TToken Contract<TContract>();

        [NotNull]
        TToken State(int index, [NotNull] Type stateType);

        [NotNull]
        TToken State<TState>(int index);

        [NotNull]
        TToken Tag(params object[] tags);
    }
}