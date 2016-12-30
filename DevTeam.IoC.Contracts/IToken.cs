namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IToken<out T>
    {
        T Key(IEnumerable<IKey> keys);

        T Key(params IKey[] keys);

        T Contract(params Type[] contractTypes);

        T Contract<TContract>();

        T State(int index, Type stateType);

        T State<TState>(int index);

        T Tag(params object[] tags);
    }
}