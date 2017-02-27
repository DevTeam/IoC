namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IProvider<TContract>
    {
        bool TryGet(out TContract instance);
    }
}