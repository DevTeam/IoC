namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IToken<out T>
    {
        [NotNull]
        T Key([NotNull] IEnumerable<IKey> keys);

        [NotNull]
        T Key([NotNull] params IKey[] keys);

        [NotNull]
        T Contract([NotNull] params Type[] contractTypes);

        [NotNull]
        T Contract<TContract>();

        [NotNull]
        T State(int index, [NotNull] Type stateType);

        [NotNull]
        T State<TState>(int index);

        [NotNull]
        T Tag(params object[] tags);
    }
}