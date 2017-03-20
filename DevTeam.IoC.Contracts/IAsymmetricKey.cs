namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IAsymmetricKey : IKey
    {
        bool Resolving { get; }
    }
}
