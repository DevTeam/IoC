namespace DevTeam.IoC.Contracts
{
    public interface IResolver<out TContract>
    {
        TContract Resolve();
    }
}