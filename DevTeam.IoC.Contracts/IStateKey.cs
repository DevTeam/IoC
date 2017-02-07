namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IStateKey: IKey
    {
        int Index { get; }

        Type StateType { get; }
    }
}
