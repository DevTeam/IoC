namespace DevTeam.IoC.Contracts
{
    public interface ILifetimeContext
    {
        long ResolveId { get; }

        long ThreadId { get; }
    }
}