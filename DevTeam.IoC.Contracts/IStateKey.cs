namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IStateKey: IKey
    {
        int Index { get; }

        Type StateType { get; }
    }
}
