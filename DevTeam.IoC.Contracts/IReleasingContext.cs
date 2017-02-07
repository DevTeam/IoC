namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IReleasingContext
    {
        ICompositeKey Key { get; }
    }
}
