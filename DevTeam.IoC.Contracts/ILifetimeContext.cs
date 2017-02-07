namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface ILifetimeContext
    {
        long ResolveId { get; }

        long ThreadId { get; }
    }
}