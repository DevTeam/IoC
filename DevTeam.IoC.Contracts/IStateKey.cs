namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IStateKey : IAsymmetricKey
    {
        int Index { get; }

        Type StateType { [NotNull] get; }
    }
}
