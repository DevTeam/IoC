namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolver<out TContract>
    {
        TContract Resolve();
    }
}